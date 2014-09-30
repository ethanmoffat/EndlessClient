using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using EOLib;
using XNAControls;

namespace EndlessClient
{
	public partial class EOGame : Game
	{
		int charDeleteWarningShown = -1; //index of the character that we've shown a warning about deleting, set to -1 for no warning shown

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

		public HUD Hud { get; private set; }

		private void InitializeControls(bool reinit = false)
		{
			//set up text boxes for login
			textBoxTextures[0] = Content.Load<Texture2D>("tbBack");
			textBoxTextures[1] = Content.Load<Texture2D>("tbLeft");
			textBoxTextures[2] = Content.Load<Texture2D>("tbRight");
			textBoxTextures[3] = Content.Load<Texture2D>("cursor");

			loginUsernameTextbox = new XNATextBox(this, new Rectangle(402, 322, 140, textBoxTextures[0].Height), textBoxTextures, "Microsoft Sans Serif", 8.0f);
			loginUsernameTextbox.MaxChars = 16;
			loginUsernameTextbox.OnTabPressed += OnTabPressed;
			loginUsernameTextbox.Clicked += OnTextClicked;
			loginUsernameTextbox.OnEnterPressed += (s, e) => { MainButtonPress(loginButtons[0], e); };
			loginUsernameTextbox.DefaultText = "Username";
			loginUsernameTextbox.LeftPadding = 4;
			dispatch.Subscriber = loginUsernameTextbox;

			loginPasswordTextbox = new XNATextBox(this, new Rectangle(402, 358, 140, textBoxTextures[0].Height), textBoxTextures, "Microsoft Sans Serif", 8.0f);
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

				switch (i)
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
			lblCredits.Text = "Endless Online - C# Client\nDeveloped by Ethan Moffat\nBased on Endless Online --\n  Copyright Vult-R\n\nThanks to :\n--Sausage for eoserv + C# EO libs\n--eoserv.net community\n--Hotdog for Eodev client";

			lblVersionInfo = new XNALabel(this, new Rectangle(30, 457, 1, 1));
			lblVersionInfo.Text = string.Format("{0}.{1:000}.{2:000} - {3}:{4}", Constants.MajorVersion, Constants.MinorVersion, Constants.ClientVersion, host, port);
			lblVersionInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.0f);
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
				//don't hide dialogs if reinitializing
				if (reinit && (XNAControl.Dialogs.Contains(component as XNAControl) || XNAControl.Dialogs.Contains((component as XNAControl).TopParent)))
					continue;

				//...except for the four main buttons
				if (!mainButtons.Contains(component as XNAButton))
					component.Visible = false;
			}
			lblVersionInfo.Visible = true;

			XNAButton testingame = new XNAButton(this, new Vector2(5, 5), "in-game");
			testingame.OnClick += (s, e) => doStateChange(GameStates.PlayingTheGame);
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
					doStateChange(GameStates.CreateAccount);

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
					doStateChange(GameStates.Login);
				});
			}
			else if (sender == mainButtons[2])
			{
				currentState = GameStates.ViewCredits;
			}
			else if (sender == mainButtons[3])
			{
				if (World.Instance.Client.ConnectedAndInitialized)
					World.Instance.Client.Disconnect();
				this.Exit();
			}
			else if ((sender == backButton && currentState != GameStates.PlayingTheGame) || sender == createButtons[1] || sender == loginButtons[1])
			{
				dispatch.Subscriber = null;
				LostConnectionDialog();
				return;
			}
			else if (sender == backButton && currentState == GameStates.PlayingTheGame)
			{
				EODialog dlg = new EODialog(this, "Are you sure you want to exit the game?", "Exit game", XNADialogButtons.OkCancel, true);
				dlg.DialogClosing += (ss, ee) =>
					{
						if(ee.Result == XNADialogResult.OK)
						{
							dispatch.Subscriber = null;
							if (World.Instance.Client.ConnectedAndInitialized)
								World.Instance.Client.Disconnect();
							doStateChange(GameStates.Initial);
						}
					};
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

				doStateChange(GameStates.LoggedIn);
			}
			else if (sender == createButtons[0])
			{
				if (currentState == GameStates.CreateAccount)
				{
					foreach (XNATextBox txt in accountCreateTextBoxes)
					{
						if (txt.Text.Length == 0)
						{
							EODialog errDlg = new EODialog(this, "Some of the fields are still empty. Fill in all the fields and try again.", "Wrong input");
							return;
						}
					}

					if (accountCreateTextBoxes[1].Text != accountCreateTextBoxes[2].Text)
					{
						//Make sure passwords match
						EODialog errDlg = new EODialog(this, "The two passwords you provided are not the same, please try again.", "Wrong password");
						return;
					}

					if (accountCreateTextBoxes[1].Text.Length < 6)
					{
						//Make sure passwords are good enough
						EODialog errDlg = new EODialog(this, "For your own safety use a longer password (try 6 or more characters)", "Wrong password");
						return;
					}

					if (!System.Text.RegularExpressions.Regex.IsMatch(accountCreateTextBoxes[5].Text, //filter emails using regex
						@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b"))
					{
						EODialog errDlg = new EODialog(this, "Enter a valid email address.", "Wrong input");
						return;
					}

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
					dlg.DialogClosing += (dlg_S, dlg_E) =>
					{
						if (dlg_E.Result == XNADialogResult.NO_BUTTON_PRESSED) //NO_BUTTON_PRESSED indicates the progress bar reached 100%
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

							doStateChange(GameStates.Initial);
							EODialog success = new EODialog(this, _msg, _caption);
						}
					};

				}
				else if (currentState == GameStates.LoggedIn)
				{
					//Character_request: show create character dialog
					//Character_create: clicked ok in create character dialog
					if (!Handlers.Character.CharacterRequest())
					{
						LostConnectionDialog();
						return;
					}

					if (!Handlers.Character.CanProceed)
					{
						EODialog errDlg = new EODialog(this, "Server is not allowing you to create a character right now. This could be a bug.", "Server error");
						return;
					}

					EOCreateCharacterDialog createCharacter = new EOCreateCharacterDialog(this, textBoxTextures[3], dispatch);
					createCharacter.DialogClosing += (dlg_S, dlg_E) =>
					{
						if (dlg_E.Result == XNADialogResult.OK)
						{
							if (!Handlers.Character.CharacterCreate(createCharacter.Gender, createCharacter.HairType, createCharacter.HairColor, createCharacter.SkinColor, createCharacter.Name))
							{
								doStateChange(GameStates.Initial);
								EODialog errDlg = new EODialog(this, "The connection to the game server was lost, please try again at a later time.", "Lost connection");
								if (World.Instance.Client.ConnectedAndInitialized)
									World.Instance.Client.Disconnect();
								return;
							}

							if (!Handlers.Character.CanProceed)
							{
								if (!Handlers.Character.TooManyCharacters)
									dlg_E.CancelClose = true;
								string caption, message = Handlers.Character.ResponseMessage(out caption);
								EODialog fail = new EODialog(this, message, caption);
								return;
							}

							EODialog dlg = new EODialog(this, "Your character has been created and is ready to explore a new world.", "Character created");
							doShowCharacters();
						}
					};
				}
			}
			else if (sender == passwordChangeBtn)
			{
				EOChangePasswordDialog dlg = new EOChangePasswordDialog(this, textBoxTextures[3], dispatch);
				dlg.DialogClosing += (dlg_S, dlg_E) =>
				{
					if (!Handlers.Account.AccountChangePassword(dlg.Username, dlg.OldPassword, dlg.NewPassword))
					{
						doStateChange(GameStates.Initial);
						EODialog errDlg = new EODialog(this, "The connection to the game server was lost, please try again at a later time.", "Lost connection");
						if (World.Instance.Client.ConnectedAndInitialized)
							World.Instance.Client.Disconnect();
						return;
					}

					string caption, msg = Handlers.Account.ResponseMessage(out caption);
					EODialog response = new EODialog(this, msg, caption);

					if (!Handlers.Account.CanProceed)
					{
						dlg_E.CancelClose = true;
						return;
					}
				};
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

			int index;
			if (loginCharButtons.Contains(sender))
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
				dlg.DialogClosing += (dlgS, dlgE) =>
				{
					switch (dlgE.Result)
					{
						case XNADialogResult.OK:
							doStateChange(GameStates.PlayingTheGame);
							break;
						case XNADialogResult.NO_BUTTON_PRESSED:
						{
							EODialog dlg2 = new EODialog(this, "Login Failed.", "Error");
							if (World.Instance.Client.ConnectedAndInitialized)
								World.Instance.Client.Disconnect();
							doStateChange(GameStates.Initial);
						}
							break;
					}
				};
			}
			else if (deleteCharButtons.Contains(sender))
			{
				index = deleteCharButtons.ToList().FindIndex(x => x == sender);
				if (World.Instance.MainPlayer.CharData.Length <= index)
					return;

				if (charDeleteWarningShown != index)
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

				EODialog promptDialog = new EODialog(this, "Character \'" + World.Instance.MainPlayer.CharData[index].name + "\' is going to be deleted. Are you sure?", "Delete character", XNADialogButtons.OkCancel);
				promptDialog.DialogClosing += (dlgS, dlgE) =>
				{
					if (dlgE.Result == XNADialogResult.OK) //user clicked ok to delete their character. do the delete here.
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

		private void OnTabPressed(object sender, EventArgs e)
		{
			if (!IsActive)
				return;
			//for loginClickedGameState
			switch (currentState)
			{
				case GameStates.Login:
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
				case GameStates.CreateAccount:
					for (int i = 0; i < accountCreateTextBoxes.Length; ++i)
					{
						if (sender == accountCreateTextBoxes[i])
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
				case GameStates.Login:
					if (sender == loginUsernameTextbox)
					{
						OnTabPressed(loginPasswordTextbox, null);
					}
					else if (sender == loginPasswordTextbox)
					{
						OnTabPressed(loginUsernameTextbox, null);
					}
					break;
				case GameStates.CreateAccount:
					for (int i = 0; i < accountCreateTextBoxes.Length; ++i)
					{
						if (sender == accountCreateTextBoxes[i])
						{
							int prev = (i == 0) ? accountCreateTextBoxes.Length - 1 : i - 1;
							OnTabPressed(accountCreateTextBoxes[prev], null);
							break;
						}
					}
					break;
			}
		}
	}
}
