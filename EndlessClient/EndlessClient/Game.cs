using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EOLib.Data;
using EOLib.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using EOLib;
using XNAControls;
using CONTROLSINIT = XNAControls.XNAControls;

namespace EndlessClient
{
	/// <summary>
	/// Game states
	/// </summary>
	public enum GameStates
	{
		/// <summary>
		/// Initial State when game is launched
		/// </summary>
		Initial,
		/// <summary>
		/// State when an account is being created
		/// </summary>
		CreateAccount,
		/// <summary>
		/// State when Login button is clicked, but account is not yet authenticated
		/// </summary>
		Login,
		/// <summary>
		/// Account is authenticated. Show available characters for account
		/// </summary>
		LoggedIn,
		/// <summary>
		/// Roll credits...
		/// </summary>
		ViewCredits,
		/// <summary>
		/// In game
		/// </summary>
		PlayingTheGame
	}

	public partial class EOGame : Game
	{
		private static EOGame inst;
		/// <summary>
		/// Singleton Instance: used/disposed from main in Program.cs
		/// </summary>
		public static EOGame Instance
		{
			get { return inst ?? (inst = new EOGame()); }
		}

		private static readonly object ComponentsLock = new object();
		public new GameComponentCollection Components
		{
			get
			{
				lock (ComponentsLock)
					return base.Components;
			}
		}

		public SpriteFont DBGFont { get; private set; }

		const int WIDTH = 640;
		const int HEIGHT = 480;

		readonly Random gen = new Random();
		int currentPersonOne, currentPersonTwo;
		GameStates currentState;
		public GameStates State { get { return currentState; } }

		//Textures actually being drawn by this class (not as components)
		//Components moved to GameUI.cs (partial class)
		Texture2D[] PeopleSetOne;
		Texture2D[] PeopleSetTwo;
		Texture2D UIBackground;
		Texture2D CharacterDisp, AccountCreateSheet, LoginUIScreen;

		bool exiting;
		AutoResetEvent connectMutex;

		string host;
		int port;
		private PacketAPI m_packetAPI;
		private PacketAPICallbackManager m_callbackManager;
		public PacketAPI API { get { return m_packetAPI; } }

#if DEBUG //don't do FPS render on release builds
		private TimeSpan? lastFPSRender;
		private int localFPS;
#endif

		//--------------------------
		//***** HELPER METHODS *****
		//--------------------------
		private async Task TryConnectToServer(Action successAction)
		{
			//the mutex here should simulate the action of spamming the button.
			//no matter what, it will only do it one at a time: the mutex is only released when the bg thread ends
			if (connectMutex == null)
			{
				connectMutex = new AutoResetEvent(true);
				if (!connectMutex.WaitOne(0))
					return;
			}
			else if (!connectMutex.WaitOne(0))
			{
				return;
			}

			if (World.Instance.Client.ConnectedAndInitialized && World.Instance.Client.Connected)
			{
				successAction();
				connectMutex.Set();
				return;
			}
			
			//execute this logic on a separate thread so the game doesn't lock up while it's trying to connect to the server
			await TaskFramework.Run(() =>
			{
				try
				{
					if (World.Instance.Client.ConnectToServer(host, port))
					{
						m_packetAPI = new PacketAPI((EOClient) World.Instance.Client);

						//set up event packet handling event bindings: 
						//	some events are triggered by the server regardless of action by the client
						m_callbackManager = new PacketAPICallbackManager(m_packetAPI, this);
						m_callbackManager.AssignCallbacks();

						((EOClient) World.Instance.Client).EventDisconnect += () => m_packetAPI.Disconnect();

						InitData data;
						if (m_packetAPI.Initialize(World.Instance.VersionMajor,
							World.Instance.VersionMinor,
							World.Instance.VersionClient,
							Win32.GetHDDSerial(),
							out data))
						{
							switch (data.ServerResponse)
							{
								case InitReply.INIT_OK:
									((EOClient) World.Instance.Client).SetInitData(data);

									if (!m_packetAPI.ConfirmInit(data.emulti_e, data.emulti_d, data.clientID))
									{
										throw new Exception(); //connection failed!
									}

									World.Instance.MainPlayer.SetPlayerID(data.clientID);
									World.Instance.SetAPIHandle(m_packetAPI);
									successAction();
									break;
								default:
									string extra;
									DATCONST1 msg = m_packetAPI.GetInitResponseMessage(out extra);
									EODialog.Show(msg, extra);
									break;
							}
						}
						else
						{
							throw new Exception(); //connection failed!
						}
					}
					else
					{
						//show connection not found
						throw new Exception();
					}
				}
				catch
				{
					if (!exiting)
					{
						EODialog.Show(DATCONST1.CONNECTION_SERVER_NOT_FOUND);
					}
				}

				connectMutex.Set();
			});
		}

		public void LostConnectionDialog()
		{
			EODialog.Show(currentState == GameStates.PlayingTheGame
				? DATCONST1.CONNECTION_LOST_IN_GAME
				: DATCONST1.CONNECTION_LOST_CONNECTION);

			if (World.Instance.Client.ConnectedAndInitialized)
				World.Instance.Client.Disconnect();
			doStateChange(GameStates.Initial);
		}

		private void doShowCharacters()
		{
			//remove any existing character renderers
			List<EOCharacterRenderer> toRemove = Components.OfType<EOCharacterRenderer>().ToList();
			foreach (EOCharacterRenderer eor in toRemove)
				eor.Close();

			//show the new data
			EOCharacterRenderer[] render = new EOCharacterRenderer[World.Instance.MainPlayer.CharData.Length];
			for (int i = 0; i < World.Instance.MainPlayer.CharData.Length; ++i)
			{
				//need to get actual draw location
				//int dOrder = 0; //for debugging
				//if (render[i] != null)
				//	dOrder = render[i].DrawOrder;
				render[i] = new EOCharacterRenderer(new Vector2(395, 60 + i * 124), World.Instance.MainPlayer.CharData[i]);
			}
		}
		
		private void doStateChange(GameStates newState)
		{
			GameStates prevState = currentState;
			currentState = newState;

			if(prevState == GameStates.PlayingTheGame && currentState != GameStates.PlayingTheGame)
			{
				Hud.Dispose();
				Hud = null;
				Components.Remove(Hud);
			}
			
			List<DrawableGameComponent> toRemove = new List<DrawableGameComponent>();
			foreach (var comp in Components)
			{
				DrawableGameComponent component = comp as DrawableGameComponent;
				if (comp == null) continue;

				//don't hide dialogs
				if (component is XNAControl &&
					(XNAControl.Dialogs.Contains(component as XNAControl) ||
					XNAControl.Dialogs.Contains((component as XNAControl).TopParent)))
					continue;

				if (prevState == GameStates.PlayingTheGame && currentState != GameStates.PlayingTheGame)
				{
					toRemove.Add(component);
				}
				else
				{
					if (component is EOCharacterRenderer)
						toRemove.Add(component as EOCharacterRenderer); //this needs to be done separately because it's a foreach loop

					if (component is XNATextBox)
					{
						(component as XNATextBox).Text = "";
						(component as XNATextBox).Selected = false;
					}
					if (component != null) component.Visible = false;
				}
			}
			foreach (DrawableGameComponent comp in toRemove)
			{
				if (comp is XNAControl)
					(comp as XNAControl).Close();
				else
					comp.Dispose();
				if (Components.Contains(comp))
					Components.Remove(comp);
			}
			toRemove.Clear();

			if (prevState == GameStates.PlayingTheGame && currentState != GameStates.PlayingTheGame)
				InitializeControls(true); //reinitialize to defaults

			switch (currentState)
			{
				case GameStates.Initial:
					ResetPeopleIndices();
					foreach (XNAButton btn in mainButtons)
						btn.Visible = true;
					lblVersionInfo.Visible = true;
					break;
				case GameStates.CreateAccount:
					foreach (XNATextBox txt in accountCreateTextBoxes)
						txt.Visible = true;
					foreach (XNAButton btn in createButtons)
						btn.Visible = true;
					createButtons[0].DrawLocation = new Vector2(359, 417);
					backButton.Visible = true;
					Dispatcher.Subscriber = accountCreateTextBoxes[0];
					break;
				case GameStates.Login:
					loginUsernameTextbox.Visible = true;
					loginPasswordTextbox.Visible = true;
					foreach (XNAButton btn in loginButtons)
						btn.Visible = true;
					foreach (XNAButton btn in mainButtons)
						btn.Visible = true;
					Dispatcher.Subscriber = loginUsernameTextbox;
					lblVersionInfo.Visible = true;
					break;
				case GameStates.ViewCredits:
					foreach (XNAButton btn in mainButtons)
						btn.Visible = true;
					lblCredits.Visible = true;
					break;
				case GameStates.LoggedIn:
					backButton.Visible = true;
					createButtons[0].Visible = true;
					createButtons[0].DrawLocation = new Vector2(334, 417);

					foreach (XNAButton x in loginCharButtons)
						x.Visible = true;
					foreach (XNAButton x in deleteCharButtons)
						x.Visible = true;

					passwordChangeBtn.Visible = true;

					doShowCharacters();
					break;
				case GameStates.PlayingTheGame:
					FieldInfo[] fi = GetType().GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
					for (int i = Components.Count - 1; i >= 0; --i)
					{
						IGameComponent comp = Components[i];
						if (comp != backButton && (comp as DrawableGameComponent) != null && 
							fi.Count(_fi => _fi.GetValue(this) == comp) == 1)
						{
							(comp as DrawableGameComponent).Dispose();
							Components.Remove(comp);
						}
					}

					backButton.Visible = true;
					//note: HUD construction moved to successful welcome message in EOConnectingDialog close event handler

					break;
			}
		}

		private void ResetPeopleIndices()
		{
			currentPersonOne = gen.Next(4);
			currentPersonTwo = gen.Next(8);
		}

		//-----------------------------
		//***** DEFAULT XNA STUFF *****
		//-----------------------------

		private EOGame()
		{
			graphics = new GraphicsDeviceManager(this) {PreferredBackBufferWidth = WIDTH, PreferredBackBufferHeight = HEIGHT};
			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			try
			{
				CONTROLSINIT.Initialize(this);
				CONTROLSINIT.IgnoreEnterForDialogs = true;
				CONTROLSINIT.IgnoreEscForDialogs = true;
			}
			catch (ArgumentNullException ex)
			{
				MessageBox.Show("Something super weird happened: " + ex.Message);
				Exit();
				return;
			}

			IsMouseVisible = true;
			Dispatcher = new KeyboardDispatcher(Window);
			ResetPeopleIndices();

			try
			{
				GFXLoader.Initialize(GraphicsDevice);
				World w = World.Instance; //set up the world
				w.Init();

				host = World.Instance.Host;
				port = World.Instance.Port;
			}
			catch (WorldLoadException wle) //could be thrown from World's constructor
			{
				MessageBox.Show(wle.Message, "Error");
				Exit();
				return;
			}
			catch (ConfigStringLoadException csle)
			{
				host = World.Instance.Host;
				port = World.Instance.Port;
				switch (csle.WhichString)
				{
					case ConfigStrings.Host:
						MessageBox.Show(
							string.Format("There was an error loading the host/port from the config file. Defaults will be used: {0}:{1}",
								host, port),
							"Config Load Failed",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
						break;
					case ConfigStrings.Port:
						MessageBox.Show(
							string.Format("There was an error loading the port from the config file. Default will be used: {0}:{1}",
								host, port),
							"Config Load Failed",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
						break;
				}
			}
			catch (ArgumentException ex) //could be thrown from GFXLoader.Initialize
			{
				MessageBox.Show("Error initializing GFXLoader: " + ex.Message, "Error");
				Exit();
				return;
			}

			if(World.Instance.EIF != null && World.Instance.EIF.Version == 0)
			{
				MessageBox.Show("The item pub file you are using is using an older format of the EIF specification. Some features may not work properly. Run the file through a batch processor or use updated pub files.", "Warning");
			}

			GFXTypes curValue = 0;
			try
			{
				Array values = Enum.GetValues(typeof(GFXTypes));
				foreach (GFXTypes value in values)
				{
					curValue = value;
					//check for GFX files. Each file has a GFX 1.
					using (Texture2D throwAway = GFXLoader.TextureFromResource(value, -99))
					{
						throwAway.Name = ""; //no-op to keep resharper happy
					}
				}
			}
			catch
			{
				MessageBox.Show(string.Format("There was an error loading GFX{0:000}.EGF : {1}. Place all .GFX files in .\\gfx\\", (int)curValue, curValue), "Error");
				Exit();
				return;
			}

			try
			{
				SoundManager = new EOSoundManager();
			}
			catch(Exception ex)
			{
				MessageBox.Show(
					string.Format("There was an error (type: {2}) initializing the sound manager: {0}\n\nCall Stack:\n {1}", ex.Message,
						ex.StackTrace, ex.GetType()), "Sound Manager Error");
				Exit();
				return;
			}

			if (World.Instance.MusicEnabled)
			{
				SoundManager.PlayBackgroundMusic(1); //mfx001 == main menu theme
			}
			
			base.Initialize();
		}

		protected override void LoadContent()
		{
			//the content (pun intended) of this method is organized by the control being instantiated
			//maybe split it off into separate "helper" functions for organization? :-/

			spriteBatch = new SpriteBatch(GraphicsDevice);

			DBGFont = Content.Load<SpriteFont>("dbg");

			//texture for UI background image
			UIBackground = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 30 + gen.Next(7));

			PeopleSetOne = new Texture2D[4];
			PeopleSetTwo = new Texture2D[8];
			//the large character drawings
			for (int i = 1; i <= 4; ++i)
			{
				PeopleSetOne[i - 1] = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 40 + i, true);
				//8 graphics in the second set of people: 61-68
				PeopleSetTwo[i - 1] = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 60 + i, true);
				PeopleSetTwo[i + 3] = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 64 + i, true);
			}
			
			//the username/password prompt background
			LoginUIScreen = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 2);
			//the character display background w/o login+delete buttons
			CharacterDisp = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 11);
			//the account create sheet w/labels for text fields
			AccountCreateSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 12, true);

			InitializeControls();
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin();

			if(currentState != GameStates.PlayingTheGame)
				spriteBatch.Draw(UIBackground, new Rectangle(0, 0, WIDTH, HEIGHT), null, Color.White);

			Rectangle personOneRect = new Rectangle(229, 70, PeopleSetOne[currentPersonOne].Width, PeopleSetOne[currentPersonOne].Height);
			Rectangle personTwoRect = new Rectangle(43, 140, PeopleSetTwo[currentPersonTwo].Width, PeopleSetTwo[currentPersonTwo].Height);
			switch (currentState)
			{
				case GameStates.Login:
					spriteBatch.Draw(PeopleSetOne[currentPersonOne], personOneRect, Color.White);
					spriteBatch.Draw(LoginUIScreen, new Vector2(266, 291), Color.White);
					break;
				case GameStates.Initial:
					spriteBatch.Draw(PeopleSetOne[currentPersonOne], personOneRect, Color.White);
					break;
				case GameStates.CreateAccount:
					spriteBatch.Draw(PeopleSetTwo[currentPersonTwo], personTwoRect, Color.White);
					//there are six labels
					for (int srcYIndex = 0; srcYIndex < 6; ++srcYIndex)
					{
						Vector2 lblpos = new Vector2(358, (srcYIndex < 3 ? 50 : 241) + (srcYIndex < 3 ? srcYIndex * 51 : (srcYIndex - 3) * 51));
						spriteBatch.Draw(AccountCreateSheet, lblpos, new Rectangle(0, srcYIndex * (srcYIndex < 2 ? 14 : 15), 149, 15), Color.White);
					}
					break;
				case GameStates.LoggedIn:
					//334, 36
					//334 160
					spriteBatch.Draw(PeopleSetTwo[currentPersonTwo], personTwoRect, Color.White);
					for (int i = 0; i < 3; ++i)
						spriteBatch.Draw(CharacterDisp, new Vector2(334, 36 + i * 124), Color.White);
					break;
			}

			spriteBatch.End();

#if DEBUG
			if (lastFPSRender == null)
				lastFPSRender = gameTime.TotalGameTime;

			localFPS++;
			if (gameTime.TotalGameTime.TotalMilliseconds - lastFPSRender.Value.TotalMilliseconds > 1000)
			{
				World.FPS = localFPS;
				localFPS = 0;
				lastFPSRender = gameTime.TotalGameTime;
			}
#endif
			base.Draw(gameTime);
		}

		//-------------------
		//***** CLEANUP *****
		//-------------------

		protected override void Dispose(bool disposing)
		{
			if (!World.Initialized)
				return;

			if (World.Instance.Client.ConnectedAndInitialized)
				World.Instance.Client.Disconnect();

			if(loginUsernameTextbox != null)
				loginUsernameTextbox.Dispose();
			if(loginPasswordTextbox != null)
				loginPasswordTextbox.Dispose();

			foreach (XNAButton btn in mainButtons)
			{
				if(btn != null)
					btn.Dispose();
			}
			foreach (XNAButton btn in loginButtons)
			{
				if(btn != null)
					btn.Dispose();
			}
			foreach (XNAButton btn in createButtons)
			{
				if(btn != null)
					btn.Dispose();
			}

			foreach (XNAButton btn in loginCharButtons)
			{
				if(btn != null)
					btn.Dispose();
			}

			if(passwordChangeBtn != null)
				passwordChangeBtn.Dispose();

			if(backButton != null)
				backButton.Dispose();

			if(lblCredits != null)
				lblCredits.Dispose();

			foreach (XNATextBox btn in accountCreateTextBoxes)
			{
				if(btn != null)
					btn.Dispose();
			}

			if(spriteBatch != null)
				spriteBatch.Dispose();
			((IDisposable)graphics).Dispose();
			
			if(lblVersionInfo != null)
				lblVersionInfo.Dispose();

			if(connectMutex != null)
				connectMutex.Dispose();

			GFXLoader.Cleanup();

			World.Instance.Dispose();

			base.Dispose(disposing);
		}

		//make sure a pending connection request terminates properly without crashing the game
		protected override void OnExiting(object sender, EventArgs args)
		{
			exiting = true;
			if(connectMutex != null)
				connectMutex.Set();
			World.Instance.Client.Dispose(); //kill pending connection request on exit
			base.OnExiting(sender, args);
		}
	}
}
