using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using EOLib;
using EOLib.Data;
using XNAControls;

namespace EndlessClient
{
	/// <summary>
	/// Game states for the menus outside of game play.
	/// </summary>
	public enum GameLoginStates
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

	public class GameLogin : Game
	{
		const int WIDTH = 640;
		const int HEIGHT = 480;
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		XNATextBox loginUsernameTextbox;
		XNATextBox loginPasswordTextbox;
		Texture2D[] textBoxTextures = new Texture2D[4];
		KeyboardDispatcher dispatch;

		XNAButton[] mainButtons = new XNAButton[4];
		XNAButton[] loginButtons = new XNAButton[2];
		XNAButton[] createButtons = new XNAButton[2];

		XNAButton[] loginCharButtons = new XNAButton[3];
		XNAButton[] deleteCharButtons = new XNAButton[3];

		XNAButton passwordChangeBtn;

		XNAButton backButton;

		XNALabel lblCredits, lblVersionInfo;

		XNATextBox[] accountCreateTextBoxes = new XNATextBox[6];

		Random gen = new Random();
		int currentPersonOne, currentPersonTwo;
		int charDeleteWarningShown = -1;
		GameLoginStates currentState;

		//Textures actually being drawn by this class (not as components)
		Texture2D[] PeopleSetOne;
		Texture2D[] PeopleSetTwo;
		Texture2D UIBackground;
		Texture2D CharacterDisp, AccountCreateSheet, LoginUIScreen;

		private void OnTabPressed(object sender, EventArgs e)
		{
			if (!IsActive)
				return;
			//for loginClickedGameState
			switch (currentState)
			{
				case GameLoginStates.Login:
					if (sender == loginUsernameTextbox)
					{
						loginUsernameTextbox.Selected = false;
						dispatch.Subscriber = loginPasswordTextbox;
						loginPasswordTextbox.Selected = true;
					}
					else
					{
						loginUsernameTextbox.Selected = true;
						dispatch.Subscriber = loginUsernameTextbox;
						loginPasswordTextbox.Selected = false;
					}
					break;
				case GameLoginStates.CreateAccount:
					for(int i = 0; i < accountCreateTextBoxes.Length; ++i)
					{
						if(sender == accountCreateTextBoxes[i])
						{
							accountCreateTextBoxes[i].Selected = false;
							int next = (i == accountCreateTextBoxes.Length - 1) ? 0 : i + 1;
							dispatch.Subscriber = accountCreateTextBoxes[next];
							accountCreateTextBoxes[next].Selected = true;
							break;
						}
					}
					break;
			}
		}

		private void OnTextClicked(object sender, EventArgs e)
		{
			switch (currentState)
			{
				case GameLoginStates.Login:
					if (sender == loginUsernameTextbox)
					{
						OnTabPressed(loginPasswordTextbox, null);
					}
					else if (sender == loginPasswordTextbox)
					{
						OnTabPressed(loginUsernameTextbox, null);
					}
					break;
				case GameLoginStates.CreateAccount:
					for (int i = 0; i < accountCreateTextBoxes.Length;++i)
					{
						if(sender == accountCreateTextBoxes[i])
						{
							int prev = (i == 0) ? accountCreateTextBoxes.Length - 1 : i - 1;
							OnTabPressed(accountCreateTextBoxes[prev], null);
							break;
						}
					}
					break;
			}
		}

		private void TryConnectToServer(Action successAction)
		{
			if (World.Instance.Client.Connected)
			{
				successAction();
				return;
			}

			new System.Threading.Thread(() =>
			{				
				try
				{
					if (!World.Instance.Client.ConnectToServer(Constants.Host, Constants.Port))
					{
						string caption, msg = Handlers.Init.ResponseMessage(out caption);
						EODialog err = new EODialog(this, msg, caption);
						return;
					}
					successAction();
				}
				catch
				{
					EODialog dlg = new EODialog(this, "The game server could not be found. Please try again at a later time", "Could not find server");
				}
			}).Start();
		}

		private void LostConnectionDialog()
		{
			//Eventually these message strings should be loaded from the global constant class, or from dat files somehow. for now this method will do.
			EODialog errDlg = new EODialog(this, "The connection to the game server was lost, please try again at a later time.", "Lost connection");
			if (World.Instance.Client.Connected)
				World.Instance.Client.Disconnect();
			doStateChange(GameLoginStates.Initial);
		}

		private void doShowCharacters()
		{
			//remove any existing character renderers
			List<EOCharacterRenderer> toRemove = new List<EOCharacterRenderer>();
			foreach(IGameComponent comp in Components)
			{
				if (comp is EOCharacterRenderer)
					toRemove.Add(comp as EOCharacterRenderer);
			}
			foreach (EOCharacterRenderer eor in toRemove)
				eor.Close();

			//show the new data
			EOCharacterRenderer[] render = new EOCharacterRenderer[World.Instance.MainPlayer.CharData.Length];
			for (int i = 0; i < World.Instance.MainPlayer.CharData.Length; ++i)
			{
				//need to get actual draw location
				render[i] = new EOCharacterRenderer(this, new Vector2(395, 60 + i * 124), World.Instance.MainPlayer.CharData[i], true);
			}
		}
		
		private void doStateChange(GameLoginStates newState)
		{
			currentState = newState;
			
			List<EOCharacterRenderer> toRemove = new List<EOCharacterRenderer>();
			foreach (DrawableGameComponent component in Components)
			{
				//don't hide dialogs
				if (XNAControl.ModalDialogs.Contains(component as XNAControl) ||
					XNAControl.ModalDialogs.Contains((component as XNAControl).TopParent))
					continue;

				if (component is EOCharacterRenderer)
					toRemove.Add(component as EOCharacterRenderer); //this needs to be done separately because it's a foreach loop

				if (component is XNATextBox)
					(component as XNATextBox).Text = "";
				
				component.Visible = false;
			}
			foreach (EOCharacterRenderer eor in toRemove)
			{
				eor.Close();
				eor.Dispose();
			}
			toRemove.Clear();

			switch (currentState)
			{
				case GameLoginStates.Initial:
					ResetPeopleIndices();
					foreach (XNAButton btn in mainButtons)
						btn.Visible = true;
					lblVersionInfo.Visible = true;
					break;
				case GameLoginStates.CreateAccount:
					foreach (XNATextBox txt in accountCreateTextBoxes)
						txt.Visible = true;
					foreach (XNAButton btn in createButtons)
						btn.Visible = true;
					createButtons[0].DrawLocation = new Vector2(359, 417);
					backButton.Visible = true;
					dispatch.Subscriber = accountCreateTextBoxes[0];
					break;
				case GameLoginStates.Login:
					loginUsernameTextbox.Visible = true;
					loginPasswordTextbox.Visible = true;
					foreach (XNAButton btn in loginButtons)
						btn.Visible = true;
					foreach (XNAButton btn in mainButtons)
						btn.Visible = true;
					dispatch.Subscriber = loginUsernameTextbox;
					break;
				case GameLoginStates.ViewCredits:
					foreach (XNAButton btn in mainButtons)
						btn.Visible = true;
					lblCredits.Visible = true;
					break;
				case GameLoginStates.LoggedIn:
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
			}
		}

		//Pretty much controls how states transition between one another
		private void MainButtonPress(object sender, EventArgs e)
		{
			if (!IsActive)
				return;
			
			//switch on sender
			if (sender == mainButtons[0])
			{
				//try connect
				//if successful go to account creation state
				TryConnectToServer(() =>
				{
					doStateChange(GameLoginStates.CreateAccount);

					EOScrollingDialog createAccountDlg = new EOScrollingDialog(this, "");
					string message = "It is very important that you enter your correct, real name, location and email address when creating an account. ";
					message += "Our system will ask you to enter your real name and email address in case you have forgotten your password.\n\n";
					message += "A lot of players who forgot their password, and signed up using fake details, have been unsuccessful in gaining access to their account. ";
					message += "So please do not make the same mistake; use real details to sign up for an account.\n\n";
					message += "Your information will only be used for recovering lost passwords. Your privacy is important to us.";
					createAccountDlg.MessageText = message;
				});
			}
			else if (sender == mainButtons[1])
			{
				//try connect
				//if successful go to account login state
				TryConnectToServer(() =>
					{
						doStateChange(GameLoginStates.Login);
					});
			}
			else if (sender == mainButtons[2])
			{
				currentState = GameLoginStates.ViewCredits;
			}
			else if (sender == mainButtons[3])
			{
				if (World.Instance.Client.Connected)
					World.Instance.Client.Disconnect();
				this.Exit();
			}
			else if (sender == backButton || sender == createButtons[1] || sender == loginButtons[1])
			{
				dispatch.Subscriber = null;
				LostConnectionDialog();
				return;
			}
			else if (sender == loginButtons[0])
			{
				if (loginUsernameTextbox.Text == "" || loginPasswordTextbox.Text == "")
					return;

				if (!Handlers.Login.LoginRequest(loginUsernameTextbox.Text, loginPasswordTextbox.Text))
				{
					LostConnectionDialog();
					return;
				}

				if (!Handlers.Login.CanProceed)
				{
					string caption, msg = Handlers.Login.ResponseMessage(out caption);
					EODialog errDlg = new EODialog(this, msg, caption);
					return;
				}

				doStateChange(GameLoginStates.LoggedIn);
			}
			else if (sender == createButtons[0])
			{
				if (currentState == GameLoginStates.CreateAccount)
				{
					bool valid = true;
					foreach (XNATextBox txt in accountCreateTextBoxes)
					{
						if (txt.Text.Length == 0)
						{
							EODialog errDlg = new EODialog(this, "Some of the fields are still empty. Fill in all the fields and try again.", "Wrong input");
							valid = false;
							break;
						}
					}

					if (valid && accountCreateTextBoxes[1].Text != accountCreateTextBoxes[2].Text)
					{
						//Make sure passwords match
						EODialog errDlg = new EODialog(this, "The two passwords you provided are not the same, please try again.", "Wrong password");
						valid = false;
					}

					if (valid && accountCreateTextBoxes[1].Text.Length < 6)
					{
						//Make sure passwords are good enough
						EODialog errDlg = new EODialog(this, "For your own safety use a longer password (try 6 or more characters)", "Wrong password");
						valid = false;
					}

					if (valid && !System.Text.RegularExpressions.Regex.IsMatch(accountCreateTextBoxes[5].Text, //filter emails using regex
						@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b"))
					{
						EODialog errDlg = new EODialog(this, "Enter a valid email address.", "Wrong input");
						valid = false;
					}

					if (valid) //start trying to create the account if we pass all the tests
					{
						//separate thread: ain't nobody got time for blocking the UI!
						new System.Threading.Thread(new System.Threading.ThreadStart(() =>
							{
								if (!Handlers.Account.AccountCheckName(accountCreateTextBoxes[0].Text))
								{
									LostConnectionDialog();
									return;
								}

								if (!Handlers.Account.CanProceed)
								{
									string caption, msg = Handlers.Account.ResponseMessage(out caption);
									EODialog errDlg = new EODialog(this, msg, caption);
									return;
								}

								//show progress bar for account creation pending and THEN create the account
								EOProgressDialog dlg = new EOProgressDialog(this, "Please wait a few minutes for creation.", "Account accepted");
								dlg.CloseAction = (bool finished) =>
								{
									if (finished) //progress bar reached 100%
									{
										if (!Handlers.Account.AccountCreate(accountCreateTextBoxes[0].Text,
											accountCreateTextBoxes[1].Text,
											accountCreateTextBoxes[3].Text,
											accountCreateTextBoxes[4].Text,
											accountCreateTextBoxes[5].Text))
										{
											LostConnectionDialog();
											return;
										}

										string _caption, _msg = Handlers.Account.ResponseMessage(out _caption);
										if (!Handlers.Account.CanProceed)
										{
											EODialog errDlg = new EODialog(this, _msg, _caption);
											return;
										}

										doStateChange(GameLoginStates.Initial);
										EODialog success = new EODialog(this, _msg, _caption);
									}
								};
							})).Start();
					}
				}
				else if (currentState == GameLoginStates.LoggedIn)
				{
					//Character_request: show create character dialog
					//Character_create: clicked ok in create character dialog
					if(!Handlers.Character.CharacterRequest())
					{
						LostConnectionDialog();
						return;
					}

					if(!Handlers.Character.CanProceed)
					{
						EODialog errDlg = new EODialog(this, "Server is not allowing you to create a character right now. This could be a bug.", "Server error");
						return;
					}

					EOCreateCharacterDialog createCharacter = new EOCreateCharacterDialog(this, textBoxTextures[3], dispatch);
					createCharacter.CloseAction = (finished) =>
					{
						if (finished)
						{
							doStateChange(GameLoginStates.Initial);
						}
						else
						{
							if (currentState == GameLoginStates.LoggedIn)
								doShowCharacters();
						}
					};
				}
			}
			else if (sender == passwordChangeBtn)
			{
				EOChangePasswordDialog dlg = new EOChangePasswordDialog(this, textBoxTextures[3], dispatch);
				dlg.CloseAction = (finished) => { if (finished) doStateChange(GameLoginStates.Initial); };
			}
		}

		private void CharModButtonPress(object sender, EventArgs e)
		{
			//click delete once: pop up initial dialog, set that initial dialog has been shown
			//Character_take: delete clicked, then dialog pops up
			//Character_remove: click ok in yes/no dialog

			//click login: send WELCOME_REQUEST, get WELCOME_REPLY
			//Send WELCOME_AGREE for map/pubs if needed
			//Send WELCOME_MSG, get WELCOME_REPLY
			//log in if all okay

			int index = 0;
			if(loginCharButtons.Contains(sender))
			{
				index = loginCharButtons.ToList().FindIndex(x => x == sender);
				if (World.Instance.MainPlayer.CharData.Length <= index)
					return;

				if (!Handlers.Welcome.SelectCharacter(World.Instance.MainPlayer.CharData[index].id))
				{
					LostConnectionDialog();
					return;
				}

				//shows the connecting window
				EOConnectingDialog dlg = new EOConnectingDialog(this);
				dlg.CloseAction = (b) =>
					{
						if (b)
						{
							//doStateChange(GameLoginStates.PlayingTheGame);
							EODialog dlg2 = new EODialog(this, "It worked!", "Success");
							dlg2.CloseAction = (b2) => { doStateChange(GameLoginStates.Initial); };
						}
					};
			}
			else if(deleteCharButtons.Contains(sender))
			{
				index = deleteCharButtons.ToList().FindIndex(x => x == sender);
				if (World.Instance.MainPlayer.CharData.Length <= index)
					return;

				if(charDeleteWarningShown != index)
				{
					EODialog warn = new EODialog(this, "Character \'" + World.Instance.MainPlayer.CharData[index].name + "\' is going to be deleted. Delete again to confirm.", "Delete character");
					charDeleteWarningShown = index;
					return;
				}

				//delete character at that index, if it exists
				if (!Handlers.Character.CharacterTake(World.Instance.MainPlayer.CharData[index].id))
				{
					LostConnectionDialog();
					return;
				}

				if (Handlers.Character.CharacterTakeID != World.Instance.MainPlayer.CharData[index].id)
				{
					EODialog warn = new EODialog(this, "The server did not respond properly for deleting the character. Try again.", "Server error");
					return;
				}

				EODialog promptDialog = new EODialog(this, "Character \'" + World.Instance.MainPlayer.CharData[index].name + "\' is going to be deleted. Are you sure?", "Delete character", XNADialog.XNADialogButtons.OkCancel);
				promptDialog.CloseAction = (okClicked) =>
				{
					if(okClicked) //user clicked ok to delete their character. do the delete here.
					{
						if (!Handlers.Character.CharacterRemove(World.Instance.MainPlayer.CharData[index].id))
						{
							LostConnectionDialog();
							return;
						}

						doShowCharacters();
					}
				};
			}
		}

		private void ResetPeopleIndices()
		{
			currentPersonOne = gen.Next(4);
			currentPersonTwo = gen.Next(8);
		}

		public GameLogin()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = WIDTH;
			graphics.PreferredBackBufferHeight = HEIGHT;
			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			IsMouseVisible = true;
			dispatch = new KeyboardDispatcher(this.Window);
			ResetPeopleIndices();
			
			try
			{
				GFXLoader.Initialize(GraphicsDevice);
				World w = World.Instance; //set up the world
			}
			catch (WorldLoadException wle) //could be thrown from World's constructor
			{
				System.Windows.Forms.MessageBox.Show(wle.Message, "Error");
				Exit();
				return;
			}
			catch (Exception ex) //could be thrown from GFXLoader.Initialize
			{
				System.Windows.Forms.MessageBox.Show("Error initializing GFXLoader: " + ex.Message, "Error");
				Exit();
				return;
			}

			if(World.Instance.EIF != null && World.Instance.EIF.Version == 0)
			{
				System.Windows.Forms.MessageBox.Show("The item pub file you are using is using an older format of the EIF specification. Some features may not work properly. Run the file through a batch processor or use updated pub files.", "Warning");
			}

			GFXTypes curValue = (GFXTypes)0;
			try
			{
				Array values = Enum.GetValues(typeof(GFXTypes));
				foreach (GFXTypes value in values)
				{
					curValue = value;
					using (Texture2D throwAway = GFXLoader.TextureFromResource(value, -99, false)) { }
				}
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show(string.Format("There was an error loading GFX{0:000}.EGF : {1}. Place all .GFX files in .\\gfx\\", (int)curValue, curValue.ToString()), "Error");
				Exit();
				return;
			}
			
			base.Initialize();
		}

		protected override void Dispose(bool disposing)
		{
			if (!World.Initialized)
				return;

			if (World.Instance.Client.Connected)
				World.Instance.Client.Disconnect();
			World.Instance.Client.Dispose();

			loginUsernameTextbox.Dispose();
			loginPasswordTextbox.Dispose();

			foreach (XNAButton btn in mainButtons)
				btn.Dispose();
			foreach (XNAButton btn in loginButtons)
				btn.Dispose();
			foreach (XNAButton btn in createButtons)
				btn.Dispose();
			
			foreach(XNAButton btn in loginCharButtons)
				btn.Dispose();

			passwordChangeBtn.Dispose();

			backButton.Dispose();

			lblCredits.Dispose();

			foreach (XNATextBox btn in accountCreateTextBoxes)
				btn.Dispose();

			base.Dispose(disposing);
		}

		protected override void LoadContent()
		{
			//the content (pun intended) of this method is organized by the control being instantiated
			//maybe split it off into separate "helper" functions for organization? :-/

			spriteBatch = new SpriteBatch(GraphicsDevice);

			//texture for UI background image
			Random gen = new Random();
			UIBackground = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 30 + gen.Next(7), false);

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
			LoginUIScreen = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 2, false);
			//the character display background w/o login+delete buttons
			CharacterDisp = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 11, false);
			//the account create sheet w/labels for text fields
			AccountCreateSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 12, true);

			//set up text boxes for login
			textBoxTextures[0] = Content.Load<Texture2D>("tbBack");
			textBoxTextures[1] = Content.Load<Texture2D>("tbLeft");
			textBoxTextures[2] = Content.Load<Texture2D>("tbRight");
			textBoxTextures[3] = Content.Load<Texture2D>("cursor");

			loginUsernameTextbox = new XNATextBox(this, new Rectangle(402, 322, 140, textBoxTextures[0].Height), textBoxTextures, "Arial", 8.0f);
			loginUsernameTextbox.MaxChars = 16;
			loginUsernameTextbox.OnTabPressed += OnTabPressed;
			loginUsernameTextbox.Clicked += OnTextClicked;
			loginUsernameTextbox.OnEnterPressed += (s, e) => { MainButtonPress(loginButtons[0], e); };
			loginUsernameTextbox.DefaultText = "Username";
			loginUsernameTextbox.LeftPadding = 4;
			dispatch.Subscriber = loginUsernameTextbox;

			loginPasswordTextbox = new XNATextBox(this, new Rectangle(402, 358, 140, textBoxTextures[0].Height), textBoxTextures, "Arial", 8.0f);
			loginPasswordTextbox.MaxChars = 12;
			loginPasswordTextbox.OnTabPressed += OnTabPressed;
			loginPasswordTextbox.Clicked += OnTextClicked;
			loginPasswordTextbox.OnEnterPressed += (s, e) => { MainButtonPress(loginButtons[0], e); };
			loginPasswordTextbox.PasswordBox = true;
			loginPasswordTextbox.LeftPadding = 4;
			loginPasswordTextbox.DefaultText = "Password";

			//set up primary four login buttons
			Texture2D mainButtonSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 13, true);
			for (int i = 0; i < mainButtons.Length; ++i)
			{
				int widthFactor = mainButtonSheet.Width / 2; //2: mouseOut and mouseOver textures
				int heightFactor = mainButtonSheet.Height / mainButtons.Length; //1 row per button
				Rectangle outSource = new Rectangle(0, i * heightFactor, widthFactor, heightFactor);
				Rectangle overSource = new Rectangle(widthFactor, i * heightFactor, widthFactor, heightFactor);
				mainButtons[i] = new XNAButton(this, mainButtonSheet, new Vector2(26, 278 + i * 40), outSource, overSource);
				mainButtons[i].OnClick += MainButtonPress;
			}

			//the button in the top-right for going back a screen
			Texture2D back = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 24, true);
			backButton = new XNAButton(this, back, new Vector2(589, 0), new Rectangle(0, 0, back.Width, back.Height / 2),
				new Rectangle(0, back.Height / 2, back.Width, back.Height / 2));
			backButton.OnClick += MainButtonPress;
			backButton.ClickArea = new Rectangle(4, 16, 16, 16);

			//Login/Cancel buttons for logging in
			Texture2D smallButtonSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
			loginButtons[0] = new XNAButton(this, smallButtonSheet, new Vector2(361, 389), new Rectangle(0, 0, 91, 29), new Rectangle(91, 0, 91, 29));
			loginButtons[1] = new XNAButton(this, smallButtonSheet, new Vector2(453, 389), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
			loginButtons[0].OnClick += MainButtonPress;
			loginButtons[1].OnClick += MainButtonPress;

			//6 text boxes (by default) for creating a new account.
			for (int i = 0; i < accountCreateTextBoxes.Length; ++i)
			{
				XNATextBox txt = accountCreateTextBoxes[i];
				//holy fuck! magic numbers!
				//basically, set the first  3 Y coord to start at 69  and move up by 51 each time
				//			 set the second 3 Y coord to start at 260 and move up by 51 each time
				int txtYCoord = (i < 3 ? 69 : 260) + (i < 3 ? i * 51 : (i - 3) * 51);
				txt = new XNATextBox(this, new Rectangle(358, txtYCoord, 240, textBoxTextures[0].Height), textBoxTextures, "Latha");

				switch(i)
				{
					case 0:
						txt.MaxChars = 16;
						break;
					case 1:
					case 2:
						txt.PasswordBox = true;
						txt.MaxChars = 12;
						break;
					default:
						txt.MaxChars = 35;
						break;
				}

				txt.DefaultText = " ";

				txt.OnTabPressed += OnTabPressed;
				txt.Clicked += OnTextClicked;
				accountCreateTextBoxes[i] = txt;
			}
			

			//create account / cancel
			Texture2D secondaryButtons = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 14, true);
			createButtons[0] = new XNAButton(this, secondaryButtons, new Vector2(359, 417), new Rectangle(0, 0, 120, 40), new Rectangle(120, 0, 120, 40));
			createButtons[1] = new XNAButton(this, secondaryButtons, new Vector2(481, 417), new Rectangle(0, 40, 120, 40), new Rectangle(120, 40, 120, 40));
			createButtons[0].OnClick += MainButtonPress;
			createButtons[1].OnClick += MainButtonPress;

			passwordChangeBtn = new XNAButton(this, secondaryButtons, new Vector2(454, 417), new Rectangle(0, 120, 120, 40), new Rectangle(120, 120, 120, 40));
			passwordChangeBtn.OnClick += MainButtonPress;

			lblCredits = new XNALabel(this, new Rectangle(300, 260, 1, 1));
			lblCredits.Text = "Endless Online - C# Client\nDeveloped by Ethan Moffat\nBased on Endless Online --\n  Copyright Vult-R\n\nThanks to Sausage for eoserv + C# EO libs\nThanks to eoserv.net community\nThanks to Hotdog for Eodev client";

			lblVersionInfo = new XNALabel(this, new Rectangle(30, 457, 1, 1));
			lblVersionInfo.Text = string.Format("{0}.{1:000}.{2:000} - {3}", Constants.MajorVersion, Constants.MinorVersion, Constants.ClientVersion, Constants.Host);
			lblVersionInfo.Font = new System.Drawing.Font("arial", 7.0f);
			lblVersionInfo.ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xb4, 0xa0, 0x8c);

			//login/delete buttons for each character
			for (int i = 0; i < 3; ++i)
			{
				loginCharButtons[i] = new XNAButton(this, smallButtonSheet, new Vector2(495, 93 + i * 124), new Rectangle(0, 58, 91, 29), new Rectangle(91, 58, 91, 29));
				loginCharButtons[i].OnClick += CharModButtonPress;
				deleteCharButtons[i] = new XNAButton(this, smallButtonSheet, new Vector2(495, 121 + i * 124), new Rectangle(0, 87, 91, 29), new Rectangle(91, 87, 91, 29));
				deleteCharButtons[i].OnClick += CharModButtonPress;
			}
			
			//hide all the components to start with
			foreach (DrawableGameComponent component in Components)
			{
				//...except for the four main buttons
				if (!mainButtons.Contains(component as XNAButton))
					component.Visible = false;
			}
			lblVersionInfo.Visible = true;
		}

		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();
			
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin();

			spriteBatch.Draw(UIBackground, new Rectangle(0, 0, WIDTH, HEIGHT), null, Color.White);

			Rectangle personOneRect = new Rectangle(229, 70, PeopleSetOne[currentPersonOne].Width, PeopleSetOne[currentPersonOne].Height);
			Rectangle personTwoRect = new Rectangle(43, 140, PeopleSetTwo[currentPersonTwo].Width, PeopleSetTwo[currentPersonTwo].Height);
			switch (currentState)
			{
				case GameLoginStates.Login:
					spriteBatch.Draw(PeopleSetOne[currentPersonOne], personOneRect, Color.White);
					spriteBatch.Draw(LoginUIScreen, new Vector2(266, 291), Color.White);
					break;
				case GameLoginStates.Initial:
					spriteBatch.Draw(PeopleSetOne[currentPersonOne], personOneRect, Color.White);
					break;
				case GameLoginStates.CreateAccount:
					spriteBatch.Draw(PeopleSetTwo[currentPersonTwo], personTwoRect, Color.White);
					//there are six labels
					for (int srcYIndex = 0; srcYIndex < 6; ++srcYIndex)
					{
						Vector2 lblpos = new Vector2(358, (srcYIndex < 3 ? 50 : 241) + (srcYIndex < 3 ? srcYIndex * 51 : (srcYIndex - 3) * 51));
						spriteBatch.Draw(AccountCreateSheet, lblpos, new Rectangle(0, srcYIndex * (srcYIndex < 2 ? 14 : 15), 149, 15), Color.White);
					}
					break;
				case GameLoginStates.ViewCredits:
					lblCredits.Visible = true;
					break;
				case GameLoginStates.LoggedIn:
					//334, 36
					//334 160
					spriteBatch.Draw(PeopleSetTwo[currentPersonTwo], personTwoRect, Color.White);
					for (int i = 0; i < 3; ++i)
						spriteBatch.Draw(CharacterDisp, new Vector2(334, 36 + i * 124), Color.White);
					break;
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
