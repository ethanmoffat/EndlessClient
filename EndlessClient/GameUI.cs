// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EOLib;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient
{
	public partial class EOGame
	{
		private int _charDeleteWarningIndex = -1; //index of the character that we've shown a warning about deleting, set to -1 for no warning shown

		private readonly GraphicsDeviceManager _graphicsDeviceManager;
		private SpriteBatch _spriteBatch;

		private XNATextBox _loginUsernameTextbox;
		private XNATextBox _loginPasswordTextbox;
		private readonly Texture2D[] _textBoxTextures = new Texture2D[4];

		public KeyboardDispatcher Dispatcher { get; private set; }

		private readonly XNAButton[] _mainButtons = new XNAButton[4];
		private readonly XNAButton[] _loginButtons = new XNAButton[2];
		private readonly XNAButton[] _createButtons = new XNAButton[2];

		private readonly XNAButton[] _loginCharButtons = new XNAButton[3];
		private readonly XNAButton[] _deleteCharButtons = new XNAButton[3];

		private XNAButton _passwordChangeBtn;

		private XNAButton _backButton;
		private bool _backButtonPressed; //workaround so the lost connection dialog doesn't show from the client disconnect event

		private XNALabel _lblCredits, _lblVersionInfo;

		private readonly XNATextBox[] _accountCreateTextBoxes = new XNATextBox[6];

		public HUD.HUD Hud { get; private set; }
		public SoundManager SoundManager { get; private set; }

		private void InitializeControls(bool reinit = false)
		{
			//set up text boxes for login
			_textBoxTextures[0] = Content.Load<Texture2D>("tbBack");
			_textBoxTextures[1] = Content.Load<Texture2D>("tbLeft");
			_textBoxTextures[2] = Content.Load<Texture2D>("tbRight");
			_textBoxTextures[3] = Content.Load<Texture2D>("cursor");

			_loginUsernameTextbox = new XNATextBox(new Rectangle(402, 322, 140, _textBoxTextures[0].Height), _textBoxTextures, Constants.FontSize08)
			{
				MaxChars = 16,
				DefaultText = "Username",
				LeftPadding = 4
			};
			_loginUsernameTextbox.OnTabPressed += OnTabPressed;
			_loginUsernameTextbox.OnClicked += OnTextClicked;
			_loginUsernameTextbox.OnEnterPressed += (s, e) => MainButtonPress(_loginButtons[0], e);
			Dispatcher.Subscriber = _loginUsernameTextbox;

			_loginPasswordTextbox = new XNATextBox(new Rectangle(402, 358, 140, _textBoxTextures[0].Height), _textBoxTextures, Constants.FontSize08)
			{
				MaxChars = 12,
				PasswordBox = true,
				LeftPadding = 4,
				DefaultText = "Password"
			};
			_loginPasswordTextbox.OnTabPressed += OnTabPressed;
			_loginPasswordTextbox.OnClicked += OnTextClicked;
			_loginPasswordTextbox.OnEnterPressed += (s, e) => MainButtonPress(_loginButtons[0], e);

			//set up primary four login buttons
			Texture2D mainButtonSheet = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 13, true);
			for (int i = 0; i < _mainButtons.Length; ++i)
			{
				int widthFactor = mainButtonSheet.Width / 2; //2: mouseOut and mouseOver textures
				int heightFactor = mainButtonSheet.Height / _mainButtons.Length; //1 row per button
				Rectangle outSource = new Rectangle(0, i * heightFactor, widthFactor, heightFactor);
				Rectangle overSource = new Rectangle(widthFactor, i * heightFactor, widthFactor, heightFactor);
				_mainButtons[i] = new XNAButton(mainButtonSheet, new Vector2(26, 278 + i * 40), outSource, overSource);
				_mainButtons[i].OnClick += MainButtonPress;
			}

			//the button in the top-right for going back a screen
			Texture2D back = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 24, true);
			_backButton = new XNAButton(back, new Vector2(589, 0), new Rectangle(0, 0, back.Width, back.Height / 2),
				new Rectangle(0, back.Height / 2, back.Width, back.Height / 2)) { DrawOrder = 100 };
			_backButton.OnClick += MainButtonPress;
			_backButton.ClickArea = new Rectangle(4, 16, 16, 16);

			//Login/Cancel buttons for logging in
			Texture2D smallButtonSheet = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
			_loginButtons[0] = new XNAButton(smallButtonSheet, new Vector2(361, 389), new Rectangle(0, 0, 91, 29), new Rectangle(91, 0, 91, 29));
			_loginButtons[1] = new XNAButton(smallButtonSheet, new Vector2(453, 389), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
			_loginButtons[0].OnClick += MainButtonPress;
			_loginButtons[1].OnClick += MainButtonPress;

			//6 text boxes (by default) for creating a new account.
			for (int i = 0; i < _accountCreateTextBoxes.Length; ++i)
			{
				//holy fuck! magic numbers!
				//basically, set the first  3 Y coord to start at 69  and move up by 51 each time
				//			 set the second 3 Y coord to start at 260 and move up by 51 each time
				int txtYCoord = (i < 3 ? 69 : 260) + (i < 3 ? i * 51 : (i - 3) * 51);
				XNATextBox txt = new XNATextBox(new Rectangle(358, txtYCoord, 240, _textBoxTextures[0].Height), _textBoxTextures, Constants.FontSize08) { LeftPadding = 4 };

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
				txt.OnClicked += OnTextClicked;
				_accountCreateTextBoxes[i] = txt;
			}


			//create account / cancel
			Texture2D secondaryButtons = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 14, true);
			_createButtons[0] = new XNAButton(secondaryButtons, new Vector2(359, 417), new Rectangle(0, 0, 120, 40), new Rectangle(120, 0, 120, 40));
			_createButtons[1] = new XNAButton(secondaryButtons, new Vector2(481, 417), new Rectangle(0, 40, 120, 40), new Rectangle(120, 40, 120, 40));
			_createButtons[0].OnClick += MainButtonPress;
			_createButtons[1].OnClick += MainButtonPress;

			_passwordChangeBtn = new XNAButton(secondaryButtons, new Vector2(454, 417), new Rectangle(0, 120, 120, 40), new Rectangle(120, 120, 120, 40));
			_passwordChangeBtn.OnClick += MainButtonPress;

			_lblCredits = new XNALabel(new Rectangle(300, 260, 1, 1), Constants.FontSize10)
			{
				Text = @"Endless Online - C# Client
Developed by Ethan Moffat
Based on Endless Online --
Copyright Vult-R

Thanks to :
--Sausage for eoserv + C# EO libs
--eoserv.net community
--Hotdog for Eodev client"
			};

			_lblVersionInfo = new XNALabel(new Rectangle(25, 453, 1, 1), Constants.FontSize07)
			{
				Text = string.Format("{0}.{1:000}.{2:000} - {3}:{4}", OldWorld.Instance.VersionMajor, OldWorld.Instance.VersionMinor, OldWorld.Instance.VersionClient, host, port),
				ForeColor = Constants.BeigeText
			};

			//login/delete buttons for each character
			for (int i = 0; i < 3; ++i)
			{
				_loginCharButtons[i] = new XNAButton(smallButtonSheet, new Vector2(495, 93 + i * 124), new Rectangle(0, 58, 91, 29), new Rectangle(91, 58, 91, 29));
				_loginCharButtons[i].OnClick += CharModButtonPress;
				_deleteCharButtons[i] = new XNAButton(smallButtonSheet, new Vector2(495, 121 + i * 124), new Rectangle(0, 87, 91, 29), new Rectangle(91, 87, 91, 29));
				_deleteCharButtons[i].OnClick += CharModButtonPress;
			}

			//hide all the components to start with
			foreach (IGameComponent iGameComp in Components)
			{
				DrawableGameComponent component = iGameComp as DrawableGameComponent;
				//don't hide dialogs if reinitializing
				if (reinit && (XNAControl.Dialogs.Contains(component as XNAControl) || 
					(component as XNAControl != null && XNAControl.Dialogs.Contains((component as XNAControl).TopParent))))
					continue;

				//...except for the four main buttons
				if (component != null && !_mainButtons.Contains(component as XNAButton))
					component.Visible = false;
			}
			_lblVersionInfo.Visible = true;

#if DEBUG
			//testinggame will login as testuser and login as the first character
			XNAButton testingame = new XNAButton(new Vector2(5, 5), "in-game", Constants.FontSize10);
			testingame.OnClick += testInGame_click;
			testingame.Visible = ConfigurationManager.AppSettings["auto_login_user"] != null &&
								 ConfigurationManager.AppSettings["auto_login_pass"] != null;
#endif
		}

#if DEBUG
		private async void testInGame_click(object sender, EventArgs e)
		{
			MainButtonPress(_mainButtons[1], e); //press login
			await Task.Delay(500);
			if (!OldWorld.Instance.Client.ConnectedAndInitialized)
				return;
			_loginUsernameTextbox.Text = ConfigurationManager.AppSettings["auto_login_user"];
			_loginPasswordTextbox.Text = ConfigurationManager.AppSettings["auto_login_pass"];

			MainButtonPress(_loginButtons[0], e); //login as acc testuser
			await Task.Delay(500);
			CharModButtonPress(_loginCharButtons[0], e); //login as char testuser
		}
#endif

		//Pretty much controls how states transition between one another
		private async void MainButtonPress(object sender, EventArgs e)
		{
			if (!IsActive)
				return;

			if (OldWorld.Instance.SoundEnabled && _mainButtons.Contains(sender))
			{
				SoundManager.GetSoundEffectRef(SoundEffectID.ButtonClick).Play();
			}

			//switch on sender
			if (sender == _mainButtons[0])
			{
				//try connect
				//if successful go to account creation state
				await TryConnectToServer(() =>
				{
					doStateChange(GameStates.CreateAccount);

					ScrollingMessageDialog createAccountDlg = new ScrollingMessageDialog("");
					string message = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int)DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_1];
					message += "\n\n";
					message += OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int)DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_2];
					message += "\n\n";
					message += OldWorld.Instance.DataFiles[OldWorld.Instance.Localized2].Data[(int)DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_3];
					createAccountDlg.MessageText = message;
				});
			}
			else if (sender == _mainButtons[1])
			{
				//try connect
				//if successful go to account login state
				await TryConnectToServer(() => doStateChange(GameStates.Login));
			}
			else if (sender == _mainButtons[2])
			{
				doStateChange(GameStates.ViewCredits);
			}
			else if (sender == _mainButtons[3])
			{
				if (OldWorld.Instance.Client.ConnectedAndInitialized)
					OldWorld.Instance.Client.Disconnect();
				Exit();
			}
			else if ((sender == _backButton && State != GameStates.PlayingTheGame) || sender == _createButtons[1] || sender == _loginButtons[1])
			{
				Dispatcher.Subscriber = null;
				DoShowLostConnectionDialogAndReturnToMainMenu();
				//disabled warning: in case I add code later below, need to remember that this should immediately return
// ReSharper disable once RedundantJumpStatement
				return;
			}
			else if (sender == _backButton && State == GameStates.PlayingTheGame)
			{
				EOMessageBox.Show(DATCONST1.EXIT_GAME_ARE_YOU_SURE, XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader, 
					(ss, ee) =>
					{
						if(ee.Result == XNADialogResult.OK)
						{
							_backButtonPressed = true;
							Dispatcher.Subscriber = null;
							OldWorld.Instance.ResetGameElements();
							if (OldWorld.Instance.Client.ConnectedAndInitialized)
								OldWorld.Instance.Client.Disconnect();
							doStateChange(GameStates.Initial);
							_backButtonPressed = false;
						}
					});
			}
			else if (sender == _loginButtons[0])
			{
				if (_loginUsernameTextbox.Text == "" || _loginPasswordTextbox.Text == "")
					return;

				LoginReply reply;
				CharacterLoginData[] dataArray;
				if (!_packetAPI.LoginRequest(_loginUsernameTextbox.Text, _loginPasswordTextbox.Text, out reply, out dataArray))
				{
					DoShowLostConnectionDialogAndReturnToMainMenu();
					return;
				}

				if (reply != LoginReply.Ok)
				{
					EOMessageBox.Show(_packetAPI.LoginResponseMessage());
					return;
				}
				OldWorld.Instance.MainPlayer.SetAccountName(_loginUsernameTextbox.Text);
				OldWorld.Instance.MainPlayer.ProcessCharacterData(dataArray);

				doStateChange(GameStates.LoggedIn);
			}
			else if (sender == _createButtons[0])
			{
				switch (State)
				{
					case GameStates.CreateAccount:
					{
						if (_accountCreateTextBoxes.Any(txt => txt.Text.Length == 0))
						{
							EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_FIELDS_STILL_EMPTY);
							return;
						}

						if (_accountCreateTextBoxes[0].Text.Length < 4)
						{
							EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_NAME_TOO_SHORT);
							return;
						}

						if (_accountCreateTextBoxes[0].Text.Distinct().Count() < 3)
						{
							EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_NAME_TOO_OBVIOUS);
							return;
						}

						if (_accountCreateTextBoxes[1].Text != _accountCreateTextBoxes[2].Text)
						{
							EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);
							return;
						}

						if (_accountCreateTextBoxes[1].Text.Length < 6)
						{
							EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);
							return;
						}

						if (_accountCreateTextBoxes[1].Text.Distinct().Count() < 3)
						{
							EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_OBVIOUS);
							return;
						}

						if (!System.Text.RegularExpressions.Regex.IsMatch(_accountCreateTextBoxes[5].Text, //filter emails using regex
							@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b"))
						{
							EOMessageBox.Show(DATCONST1.ACCOUNT_CREATE_EMAIL_INVALID);
							return;
						}

						AccountReply reply;
						if (!_packetAPI.AccountCheckName(_accountCreateTextBoxes[0].Text, out reply))
						{
							DoShowLostConnectionDialogAndReturnToMainMenu();
							return;
						}

						if (reply != AccountReply.Continue)
						{
							EOMessageBox.Show(_packetAPI.AccountResponseMessage());
							return;
						}

						//show progress bar for account creation pending and THEN create the account
						string pbmessage = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized1].Data[(int) DATCONST1.ACCOUNT_CREATE_ACCEPTED + 1];
						string pbcaption = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized1].Data[(int) DATCONST1.ACCOUNT_CREATE_ACCEPTED];
						ProgressDialog dlg = new ProgressDialog(pbmessage, pbcaption);
						dlg.DialogClosing += (dlg_S, dlg_E) =>
						{
							if (dlg_E.Result != XNADialogResult.NO_BUTTON_PRESSED) return;

							if (!_packetAPI.AccountCreate(_accountCreateTextBoxes[0].Text,
								_accountCreateTextBoxes[1].Text,
								_accountCreateTextBoxes[3].Text,
								_accountCreateTextBoxes[4].Text,
								_accountCreateTextBoxes[5].Text,
								HDDSerial.GetHDDSerial(),
								out reply))
							{
								DoShowLostConnectionDialogAndReturnToMainMenu();
								return;
							}

							DATCONST1 resource = _packetAPI.AccountResponseMessage();
							if (reply != AccountReply.Created)
							{
								EOMessageBox.Show(resource);
								return;
							}

							doStateChange(GameStates.Initial);
							EOMessageBox.Show(resource);
						};

					}
						break;
					case GameStates.LoggedIn:
					{
						//Character_request: show create character dialog
						//Character_create: clicked ok in create character dialog
						CharacterReply reply;
						if (!_packetAPI.CharacterRequest(out reply))
						{
							DoShowLostConnectionDialogAndReturnToMainMenu();
							return;
						}

						if (reply != CharacterReply.Ok)
						{
							EOMessageBox.Show("Server is not allowing you to create a character right now. This could be a bug.", "Server error");
							return;
						}

						EOCreateCharacterDialog createCharacter = new EOCreateCharacterDialog(_textBoxTextures[3], Dispatcher);
						createCharacter.DialogClosing += (dlg_S, dlg_E) =>
						{
							if (dlg_E.Result != XNADialogResult.OK) return;

							CharacterLoginData[] dataArray;
							if (!_packetAPI.CharacterCreate(createCharacter.Gender, createCharacter.HairType, createCharacter.HairColor, createCharacter.SkinColor, createCharacter.Name, out reply, out dataArray))
							{
								DoShowLostConnectionDialogAndReturnToMainMenu();
								return;
							}

							if (reply != CharacterReply.Ok)
							{
								if (reply != CharacterReply.Full)
									dlg_E.CancelClose = true;
								EOMessageBox.Show(_packetAPI.CharacterResponseMessage());
								return;
							}

							EOMessageBox.Show(DATCONST1.CHARACTER_CREATE_SUCCESS);
							OldWorld.Instance.MainPlayer.ProcessCharacterData(dataArray);
							doShowCharacters();
						};
					}
						break;
				}
			}
			else if (sender == _passwordChangeBtn)
			{
				ChangePasswordDialog dlg = new ChangePasswordDialog(_textBoxTextures[3], Dispatcher);
				dlg.DialogClosing += (dlg_S, dlg_E) =>
				{
					if (dlg_E.Result != XNADialogResult.OK) return;

					AccountReply reply;
					if (!_packetAPI.AccountChangePassword(dlg.Username, dlg.OldPassword, dlg.NewPassword, out reply))
					{
						DoShowLostConnectionDialogAndReturnToMainMenu();
						return;
					}

					EOMessageBox.Show(_packetAPI.AccountResponseMessage());

					if (reply == AccountReply.ChangeSuccess) return;
					dlg_E.CancelClose = true;
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
			if (_loginCharButtons.Contains(sender))
			{
				index = _loginCharButtons.ToList().FindIndex(x => x == sender);
				if (OldWorld.Instance.MainPlayer.CharData == null || OldWorld.Instance.MainPlayer.CharData.Length <= index)
					return;

				WelcomeRequestData data;
				if (!_packetAPI.SelectCharacter(OldWorld.Instance.MainPlayer.CharData[index].id, out data))
				{
					DoShowLostConnectionDialogAndReturnToMainMenu();
					return;
				}

				//handles the WelcomeRequestData object
				OldWorld.Instance.ApplyWelcomeRequest(_packetAPI, data);

				//shows the connecting window
				GameLoadingDialog dlg = new GameLoadingDialog(_packetAPI);
				dlg.DialogClosing += (dlgS, dlgE) =>
				{
					switch (dlgE.Result)
					{
						case XNADialogResult.OK:
							doStateChange(GameStates.PlayingTheGame);
							
							OldWorld.Instance.ApplyWelcomeMessage(dlg.WelcomeData);
							
							Hud = new HUD.HUD(this, _packetAPI);
							Components.Add(Hud);
							Hud.SetNews(dlg.WelcomeData.News);
							Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.LOADING_GAME_HINT_FIRST);
							
							if(data.FirstTimePlayer)
								EOMessageBox.Show(DATCONST1.WARNING_FIRST_TIME_PLAYERS, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
							break;
						case XNADialogResult.NO_BUTTON_PRESSED:
						{
							EOMessageBox.Show(DATCONST1.CONNECTION_SERVER_BUSY);
							if (OldWorld.Instance.Client.ConnectedAndInitialized)
								OldWorld.Instance.Client.Disconnect();
							doStateChange(GameStates.Initial);
						}
							break;
					}
				};
			}
			else if (_deleteCharButtons.Contains(sender))
			{
				index = _deleteCharButtons.ToList().FindIndex(x => x == sender);
				if (OldWorld.Instance.MainPlayer.CharData.Length <= index)
					return;

				if (_charDeleteWarningIndex != index)
				{
					EOMessageBox.Show("Character \'" + OldWorld.Instance.MainPlayer.CharData[index].name + "\' ", DATCONST1.CHARACTER_DELETE_FIRST_CHECK);
					_charDeleteWarningIndex = index;
					return;
				}

				//delete character at that index, if it exists
				int takeID;
				if (!_packetAPI.CharacterTake(OldWorld.Instance.MainPlayer.CharData[index].id, out takeID))
				{
					DoShowLostConnectionDialogAndReturnToMainMenu();
					return;
				}

				if (takeID != OldWorld.Instance.MainPlayer.CharData[index].id)
				{
					EOMessageBox.Show("The server did not respond properly for deleting the character. Try again.", "Server error");
					return;
				}

				EOMessageBox.Show("Character \'" + OldWorld.Instance.MainPlayer.CharData[index].name + "\' ",
					DATCONST1.CHARACTER_DELETE_CONFIRM, XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogLargeHeader,
					(dlgS, dlgE) =>
					{
						if (dlgE.Result == XNADialogResult.OK) //user clicked ok to delete their character. do the delete here.
						{
							CharacterLoginData[] dataArray;
							if (!_packetAPI.CharacterRemove(OldWorld.Instance.MainPlayer.CharData[index].id, out dataArray))
							{
								DoShowLostConnectionDialogAndReturnToMainMenu();
								return;
							}

							OldWorld.Instance.MainPlayer.ProcessCharacterData(dataArray);
							doShowCharacters();
						}
					});
			}
		}

		private void OnTabPressed(object sender, EventArgs e)
		{
			if (!IsActive)
				return;
			//for loginClickedGameState
			switch (State)
			{
				case GameStates.Login:
					if (sender == _loginUsernameTextbox)
					{
						_loginUsernameTextbox.Selected = false;
						Dispatcher.Subscriber = _loginPasswordTextbox;
						_loginPasswordTextbox.Selected = true;
					}
					else
					{
						_loginUsernameTextbox.Selected = true;
						Dispatcher.Subscriber = _loginUsernameTextbox;
						_loginPasswordTextbox.Selected = false;
					}
					break;
				case GameStates.CreateAccount:
					for (int i = 0; i < _accountCreateTextBoxes.Length; ++i)
					{
						if (sender == _accountCreateTextBoxes[i])
						{
							_accountCreateTextBoxes[i].Selected = false;
							int next = (i == _accountCreateTextBoxes.Length - 1) ? 0 : i + 1;
							Dispatcher.Subscriber = _accountCreateTextBoxes[next];
							_accountCreateTextBoxes[next].Selected = true;
							break;
						}
					}
					break;
			}
		}

		private void OnTextClicked(object sender, EventArgs e)
		{
			switch (State)
			{
				case GameStates.Login:
					if (sender == _loginUsernameTextbox)
					{
						OnTabPressed(_loginPasswordTextbox, null);
					}
					else if (sender == _loginPasswordTextbox)
					{
						OnTabPressed(_loginUsernameTextbox, null);
					}
					break;
				case GameStates.CreateAccount:
					for (int i = 0; i < _accountCreateTextBoxes.Length; ++i)
					{
						if (sender == _accountCreateTextBoxes[i])
						{
							int prev = (i == 0) ? _accountCreateTextBoxes.Length - 1 : i - 1;
							OnTabPressed(_accountCreateTextBoxes[prev], null);
							break;
						}
					}
					break;
			}
		}
	}
}
