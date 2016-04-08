// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Services;
using EOLib.Net;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using CONTROLSINIT = XNAControls.XNAControls;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace EndlessClient
{
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

		public SpriteFont DBGFont { get; private set; }

		private const int WIDTH = 640;
		private const int HEIGHT = 480;

		private readonly Random _randGenerator;
		private int _currentPersonOne, _currentPersonTwo;
		public GameStates State { get; private set; }

		private Texture2D[] _peopleSetOne;
		private Texture2D[] _peopleSetTwo;
		private Texture2D _backgroundTexture;
		private Texture2D _characterSelectBackground, _accountCreateTextures, _userPassLoginPromptBackground;

		private bool _exiting;
		private AutoResetEvent _connectMutex;

		private string host;
		private int port;
		private PacketAPI _packetAPI;
		private PacketAPICallbackManager _callbackManager;
		public PacketAPI API { get { return _packetAPI; } }

		private INativeGraphicsLoader _gfxLoader;
		public INativeGraphicsManager GFXManager { get; private set; }

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
			if (_connectMutex == null)
			{
				_connectMutex = new AutoResetEvent(true);
				if (!_connectMutex.WaitOne(0))
					return;
			}
			else if (!_connectMutex.WaitOne(0))
			{
				return;
			}

			if (OldWorld.Instance.Client.ConnectedAndInitialized && OldWorld.Instance.Client.Connected)
			{
				successAction();
				_connectMutex.Set();
				return;
			}
			
			//execute this logic on a separate thread so the game doesn't lock up while it's trying to connect to the server
			await Task.Run(() =>
			{
				try
				{
					if (OldWorld.Instance.Client.ConnectToServer(host, port))
					{
						_packetAPI = new PacketAPI((EOClient) OldWorld.Instance.Client);

						//set up event packet handling event bindings: 
						//	some events are triggered by the server regardless of action by the client
						_callbackManager = new PacketAPICallbackManager(_packetAPI, this);
						_callbackManager.AssignCallbacks();

						((EOClient) OldWorld.Instance.Client).EventDisconnect += () => _packetAPI.Disconnect();

						InitData data;
						if (_packetAPI.Initialize(OldWorld.Instance.VersionMajor,
							OldWorld.Instance.VersionMinor,
							OldWorld.Instance.VersionClient,
							new HDSerialNumberService().GetHDSerialNumber(),
							out data))
						{
							switch (data.ServerResponse)
							{
								case InitReply.INIT_OK:
									((EOClient) OldWorld.Instance.Client).SetInitData(data);

									if (!_packetAPI.ConfirmInit(data.emulti_e, data.emulti_d, data.clientID))
									{
										throw new Exception(); //connection failed!
									}

									OldWorld.Instance.MainPlayer.SetPlayerID(data.clientID);
									OldWorld.Instance.SetAPIHandle(_packetAPI);
									successAction();
									break;
								default:
									string extra;
									DATCONST1 msg = _packetAPI.GetInitResponseMessage(out extra);
									EOMessageBox.Show(msg, extra);
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
					if (!_exiting)
					{
						EOMessageBox.Show(DATCONST1.CONNECTION_SERVER_NOT_FOUND);
					}
				}

				_connectMutex.Set();
			});
		}

		public void ShowLostConnectionDialog()
		{
			if (_backButtonPressed) return;
			EOMessageBox.Show(State == GameStates.PlayingTheGame
				? DATCONST1.CONNECTION_LOST_IN_GAME
				: DATCONST1.CONNECTION_LOST_CONNECTION);	
		}

		public void ResetWorldElements()
		{
			OldWorld.Instance.ResetGameElements();
		}

		public void DisconnectFromGameServer()
		{
			if (OldWorld.Instance.Client.ConnectedAndInitialized)
				OldWorld.Instance.Client.Disconnect();
		}

		public void SetInitialGameState()
		{
			doStateChange(GameStates.Initial);
		}

		public void DoShowLostConnectionDialogAndReturnToMainMenu()
		{
			ShowLostConnectionDialog();
			ResetWorldElements();
			DisconnectFromGameServer();
			SetInitialGameState();
		}

		private void doShowCharacters()
		{
			//remove any existing character renderers
			var toRemove = Components.OfType<OldCharacterRenderer>().ToList();
			foreach (OldCharacterRenderer eor in toRemove)
				eor.Close();

			//show the new data
			OldCharacterRenderer[] render = new OldCharacterRenderer[OldWorld.Instance.MainPlayer.CharData.Length];
			for (int i = 0; i < OldWorld.Instance.MainPlayer.CharData.Length; ++i)
				render[i] = new OldCharacterRenderer(new Vector2(395, 60 + i * 124), OldWorld.Instance.MainPlayer.CharData[i]);
		}
		
		private void doStateChange(GameStates newState)
		{
			//todo: clean this up, it's a mess

			GameStates prevState = State;
			State = newState;

			if (prevState == GameStates.TestMode && newState == GameStates.Initial)
			{
				var testComponents = Components.OfType<CharacterStateTest>().Cast<GameComponent>().ToList();
				testComponents = testComponents.Concat(Components.OfType<CharacterRenderer>()).ToList();
				foreach (var component in testComponents)
				{
					component.Dispose();
					Components.Remove(component);
				}
			}

			if(prevState == GameStates.PlayingTheGame && State != GameStates.PlayingTheGame)
			{
				Components.OfType<IDisposable>()
						  .ToList()
						  .ForEach(x => x.Dispose());
				Components.Clear();

				foreach (var dlg in XNAControl.Dialogs)
				{
					dlg.Visible = true;
					Components.Add(dlg);
				}
			}
			
			List<DrawableGameComponent> toRemove = new List<DrawableGameComponent>();
			foreach (var component in Components.OfType<DrawableGameComponent>())
			{
				//don't hide dialogs
				if (component is XNAControl &&
					(XNAControl.Dialogs.Contains(component as XNAControl) ||
					XNAControl.Dialogs.Contains((component as XNAControl).TopParent)))
					continue;

				if (prevState == GameStates.PlayingTheGame && State != GameStates.PlayingTheGame)
				{
					toRemove.Add(component);
				}
				else
				{
					if (component is OldCharacterRenderer)
						toRemove.Add(component as OldCharacterRenderer); //this needs to be done separately because it's a foreach loop

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

			if (prevState == GameStates.PlayingTheGame && State != GameStates.PlayingTheGame)
				InitializeControls(true); //reinitialize to defaults

			switch (State)
			{
				case GameStates.Initial:
					ResetPeopleIndices();

					foreach (XNAButton btn in _mainButtons)
						btn.Visible = true;
					_lblVersionInfo.Visible = true;

#if DEBUG
					_testInGame.Visible = ConfigurationManager.AppSettings["auto_login_user"] != null &&
										  ConfigurationManager.AppSettings["auto_login_pass"] != null;
#endif
					break;
				case GameStates.CreateAccount:
					foreach (XNATextBox txt in _accountCreateTextBoxes)
						txt.Visible = true;
					foreach (XNAButton btn in _createButtons)
						btn.Visible = true;
					_createButtons[0].DrawLocation = new Vector2(359, 417);
					_backButton.Visible = true;
					Dispatcher.Subscriber = _accountCreateTextBoxes[0];
					break;
				case GameStates.Login:
					_loginUsernameTextbox.Visible = true;
					_loginPasswordTextbox.Visible = true;
					foreach (XNAButton btn in _loginButtons)
						btn.Visible = true;
					foreach (XNAButton btn in _mainButtons)
						btn.Visible = true;
					Dispatcher.Subscriber = _loginUsernameTextbox;
					_lblVersionInfo.Visible = true;
					break;
				case GameStates.ViewCredits:
					foreach (XNAButton btn in _mainButtons)
						btn.Visible = true;
					_lblCredits.Visible = true;
					break;
				case GameStates.LoggedIn:
					_backButton.Visible = true;
					_createButtons[0].Visible = true;
					_createButtons[0].DrawLocation = new Vector2(334, 417);

					foreach (XNAButton x in _loginCharButtons)
						x.Visible = true;
					foreach (XNAButton x in _deleteCharButtons)
						x.Visible = true;

					_passwordChangeBtn.Visible = true;

					doShowCharacters();
					break;
				case GameStates.PlayingTheGame:
					FieldInfo[] fi = GetType().GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
					for (int i = Components.Count - 1; i >= 0; --i)
					{
						IGameComponent comp = Components[i];
						if (comp != _backButton && (comp as DrawableGameComponent) != null && 
							fi.Count(_fi => _fi.GetValue(this) == comp) == 1)
						{
							(comp as DrawableGameComponent).Dispose();
							Components.Remove(comp);
						}
					}

					_backButton.Visible = true;
					//note: HUD construction moved to successful welcome message in GameLoadingDialog close event handler

					break;
				case GameStates.TestMode:
					var file = new ItemFile();
					file.Load(Constants.ItemFilePath);
					var testComponent = new CharacterStateTest(this, file);
					Components.Add(testComponent);
					break;
			}
		}

		private void ResetPeopleIndices()
		{
			_currentPersonOne = _randGenerator.Next(4);
			_currentPersonTwo = _randGenerator.Next(8);
		}

		//-----------------------------
		//***** DEFAULT XNA STUFF *****
		//-----------------------------

		private EOGame()
		{
			_randGenerator = new Random();
			_graphicsDeviceManager = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = WIDTH,
				PreferredBackBufferHeight = HEIGHT
			};
			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			IsMouseVisible = true;
			Dispatcher = new KeyboardDispatcher(Window);
			ResetPeopleIndices();

			if (!InitializeXNAControls() ||
				!InitializeGFXManager() ||
				!InitializeWorld() ||
				!InitializeSoundManager())
				return;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			DBGFont = Content.Load<SpriteFont>("dbg");

			_backgroundTexture = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 30 + _randGenerator.Next(7));

			_peopleSetOne = new Texture2D[4];
			_peopleSetTwo = new Texture2D[8];

			for (int i = 1; i <= 4; ++i)
			{
				_peopleSetOne[i - 1] = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 40 + i, true);
				_peopleSetTwo[i - 1] = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 60 + i, true);
				_peopleSetTwo[i + 3] = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 64 + i, true);
			}
			
			_userPassLoginPromptBackground = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 2);
			_characterSelectBackground = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 11);
			_accountCreateTextures = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 12, true);

			InitializeControls();
		}

#if DEBUG

		private KeyboardState _lastState = Keyboard.GetState();

		protected override void Update(GameTime gameTime)
		{
			if (!IsActive || (State != GameStates.Initial && State != GameStates.TestMode))
			{
				base.Update(gameTime);
				return;
			}

			var currentState = Keyboard.GetState();

			if (State == GameStates.Initial && _lastState.IsKeyDown(Keys.F5) && currentState.IsKeyUp(Keys.F5))
			{
				doStateChange(GameStates.TestMode);
			}
			else if (State == GameStates.TestMode && _lastState.IsKeyDown(Keys.Escape) && currentState.IsKeyUp(Keys.Escape))
			{
				doStateChange(GameStates.Initial);
			}

			_lastState = currentState;

			base.Update(gameTime);
		}

#endif

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(State == GameStates.TestMode ? Color.White : Color.Black);
			_spriteBatch.Begin();

			if(State != GameStates.PlayingTheGame && State != GameStates.TestMode)
				_spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, WIDTH, HEIGHT), null, Color.White);

			Rectangle personOneRect = new Rectangle(229, 70, _peopleSetOne[_currentPersonOne].Width, _peopleSetOne[_currentPersonOne].Height);
			Rectangle personTwoRect = new Rectangle(43, 140, _peopleSetTwo[_currentPersonTwo].Width, _peopleSetTwo[_currentPersonTwo].Height);
			switch (State)
			{
				case GameStates.Login:
					_spriteBatch.Draw(_peopleSetOne[_currentPersonOne], personOneRect, Color.White);
					_spriteBatch.Draw(_userPassLoginPromptBackground, new Vector2(266, 291), Color.White);
					break;
				case GameStates.Initial:
					_spriteBatch.Draw(_peopleSetOne[_currentPersonOne], personOneRect, Color.White);
					break;
				case GameStates.CreateAccount:
					_spriteBatch.Draw(_peopleSetTwo[_currentPersonTwo], personTwoRect, Color.White);
					//there are six labels
					for (int srcYIndex = 0; srcYIndex < 6; ++srcYIndex)
					{
						Vector2 lblpos = new Vector2(358, (srcYIndex < 3 ? 50 : 241) + (srcYIndex < 3 ? srcYIndex * 51 : (srcYIndex - 3) * 51));
						_spriteBatch.Draw(_accountCreateTextures, lblpos, new Rectangle(0, srcYIndex * (srcYIndex < 2 ? 14 : 15), 149, 15), Color.White);
					}
					break;
				case GameStates.LoggedIn:
					//334, 36
					//334 160
					_spriteBatch.Draw(_peopleSetTwo[_currentPersonTwo], personTwoRect, Color.White);
					for (int i = 0; i < 3; ++i)
						_spriteBatch.Draw(_characterSelectBackground, new Vector2(334, 36 + i * 124), Color.White);
					break;
			}

			_spriteBatch.End();

#if DEBUG
			if (State != GameStates.TestMode)
			{
				if (lastFPSRender == null)
					lastFPSRender = gameTime.TotalGameTime;

				localFPS++;
				if (gameTime.TotalGameTime.TotalMilliseconds - lastFPSRender.Value.TotalMilliseconds > 1000)
				{
					OldWorld.FPS = localFPS;
					localFPS = 0;
					lastFPSRender = gameTime.TotalGameTime;
				}
			}
#endif
			base.Draw(gameTime);
		}

		private bool InitializeXNAControls()
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
				return false;
			}

			return true;
		}

		private bool InitializeGFXManager()
		{
			try
			{
				_gfxLoader = new CrossPlatformGFXLoader();
				GFXManager = new GFXManager(_gfxLoader, GraphicsDevice);
			}
			catch (LibraryLoadException lle)
			{
				MessageBox.Show(
					string.Format(
						"There was an error loading GFX{0:000}.EGF : {1}. Place all .GFX files in .\\gfx\\. The error message is:\n\n\"{2}\"",
						(int)lle.WhichGFX,
						lle.WhichGFX,
						lle.Message),
					"GFX Load Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				Exit();
				return false;
			}
			catch (ArgumentNullException ex)
			{
				MessageBox.Show("Error initializing GFXManager: " + ex.Message, "Error");
				Exit();
				return false;
			}

			return true;
		}

		private bool InitializeWorld()
		{
			try
			{
				OldWorld w = OldWorld.Instance;
				w.Init();

				host = w.Host;
				port = w.Port;
			}
			catch (WorldLoadException wle)
			{
				MessageBox.Show(wle.Message, "Error");
				Exit();
				return false;
			}
			catch (ConfigStringLoadException csle)
			{
				host = OldWorld.Instance.Host;
				port = OldWorld.Instance.Port;
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
				return false;
			}

			return true;
		}

		private bool InitializeSoundManager()
		{
			try
			{
				SoundManager = new SoundManager();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					string.Format("There was an error (type: {2}) initializing the sound manager: {0}\n\nCall Stack:\n {1}", ex.Message,
						ex.StackTrace, ex.GetType()), "Sound Manager Error");
				Exit();
				return false;
			}

			if (OldWorld.Instance.MusicEnabled)
				SoundManager.PlayBackgroundMusic(1); //mfx001 == main menu theme
			return true;
		}

		//-------------------
		//***** CLEANUP *****
		//-------------------

		protected override void Dispose(bool disposing)
		{
			if (!OldWorld.Initialized)
				return;

			if (OldWorld.Instance.Client.ConnectedAndInitialized)
				OldWorld.Instance.Client.Disconnect();

			if(_loginUsernameTextbox != null)
				_loginUsernameTextbox.Close();
			if(_loginPasswordTextbox != null)
				_loginPasswordTextbox.Close();

			foreach (XNAButton btn in _mainButtons)
			{
				if(btn != null)
					btn.Close();
			}
			foreach (XNAButton btn in _loginButtons)
			{
				if(btn != null)
					btn.Close();
			}
			foreach (XNAButton btn in _createButtons)
			{
				if(btn != null)
					btn.Close();
			}

			foreach (XNAButton btn in _loginCharButtons)
			{
				if(btn != null)
					btn.Close();
			}

			if(_passwordChangeBtn != null)
				_passwordChangeBtn.Close();

			if(_backButton != null)
				_backButton.Close();

			if(_lblCredits != null)
				_lblCredits.Close();

			foreach (XNATextBox btn in _accountCreateTextBoxes)
			{
				if(btn != null)
					btn.Close();
			}

			if(_spriteBatch != null)
				_spriteBatch.Dispose();
			((IDisposable)_graphicsDeviceManager).Dispose();
			
			if(_lblVersionInfo != null)
				_lblVersionInfo.Close();

			if(_connectMutex != null)
				_connectMutex.Dispose();

			GFXManager.Dispose();
			_gfxLoader.Dispose();

			Dispatcher.Dispose();

			OldWorld.Instance.Dispose();

			base.Dispose(disposing);
		}

		//make sure a pending connection request terminates properly without crashing the game
		protected override void OnExiting(object sender, EventArgs args)
		{
			_exiting = true;
			if(_connectMutex != null)
				_connectMutex.Set();
			OldWorld.Instance.Client.Dispose(); //kill pending connection request on exit
			base.OnExiting(sender, args);
		}
	}
}
