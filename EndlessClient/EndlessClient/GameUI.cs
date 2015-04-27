using System;
using System.Linq;
using System.Threading;
using EOLib.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using EOLib;
using XNAControls;

namespace EndlessClient
{
	public partial class EOGame
	{
		int charDeleteWarningShown = -1; //index of the character that we've shown a warning about deleting, set to -1 for no warning shown

		readonly GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		XNATextBox loginUsernameTextbox;
		XNATextBox loginPasswordTextbox;
		readonly Texture2D[] textBoxTextures = new Texture2D[4];

		public KeyboardDispatcher Dispatcher { get; private set; }

		readonly XNAButton[] mainButtons = new XNAButton[4];
		readonly XNAButton[] loginButtons = new XNAButton[2];
		readonly XNAButton[] createButtons = new XNAButton[2];

		readonly XNAButton[] loginCharButtons = new XNAButton[3];
		readonly XNAButton[] deleteCharButtons = new XNAButton[3];

		XNAButton passwordChangeBtn;

		XNAButton backButton;

		XNALabel lblCredits, lblVersionInfo;

		readonly XNATextBox[] accountCreateTextBoxes = new XNATextBox[6];

		public HUD Hud { get; private set; }
		public EOSoundManager SoundManager { get; private set; }

		private void InitializeControls(bool reinit = false)
		{
			//set up text boxes for login
			textBoxTextures[0] = Content.Load<Texture2D>("tbBack");
			textBoxTextures[1] = Content.Load<Texture2D>("tbLeft");
			textBoxTextures[2] = Content.Load<Texture2D>("tbRight");
			textBoxTextures[3] = Content.Load<Texture2D>("cursor");

			loginUsernameTextbox = new XNATextBox(new Rectangle(402, 322, 140, textBoxTextures[0].Height), textBoxTextures, "Microsoft Sans Serif", 8.0f)
			{
				MaxChars = 16,
				DefaultText = "Username",
				LeftPadding = 4
			};
			loginUsernameTextbox.OnTabPressed += OnTabPressed;
			loginUsernameTextbox.OnClicked += OnTextClicked;
			loginUsernameTextbox.OnEnterPressed += (s, e) => MainButtonPress(loginButtons[0], e);
			Dispatcher.Subscriber = loginUsernameTextbox;

			loginPasswordTextbox = new XNATextBox(new Rectangle(402, 358, 140, textBoxTextures[0].Height), textBoxTextures, "Microsoft Sans Serif", 8.0f)
			{
				MaxChars = 12,
				PasswordBox = true,
				LeftPadding = 4,
				DefaultText = "Password"
			};
			loginPasswordTextbox.OnTabPressed += OnTabPressed;
			loginPasswordTextbox.OnClicked += OnTextClicked;
			loginPasswordTextbox.OnEnterPressed += (s, e) => MainButtonPress(loginButtons[0], e);

			//set up primary four login buttons
			Texture2D mainButtonSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 13, true);
			for (int i = 0; i < mainButtons.Length; ++i)
			{
				int widthFactor = mainButtonSheet.Width / 2; //2: mouseOut and mouseOver textures
				int heightFactor = mainButtonSheet.Height / mainButtons.Length; //1 row per button
				Rectangle outSource = new Rectangle(0, i * heightFactor, widthFactor, heightFactor);
				Rectangle overSource = new Rectangle(widthFactor, i * heightFactor, widthFactor, heightFactor);
				mainButtons[i] = new XNAButton(mainButtonSheet, new Vector2(26, 278 + i * 40), outSource, overSource);
				mainButtons[i].OnClick += MainButtonPress;
			}

			//the button in the top-right for going back a screen
			Texture2D back = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 24, true);
			backButton = new XNAButton(back, new Vector2(589, 0), new Rectangle(0, 0, back.Width, back.Height / 2),
				new Rectangle(0, back.Height / 2, back.Width, back.Height / 2));
			backButton.OnClick += MainButtonPress;
			backButton.ClickArea = new Rectangle(4, 16, 16, 16);

			//Login/Cancel buttons for logging in
			Texture2D smallButtonSheet = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
			loginButtons[0] = new XNAButton(smallButtonSheet, new Vector2(361, 389), new Rectangle(0, 0, 91, 29), new Rectangle(91, 0, 91, 29));
			loginButtons[1] = new XNAButton(smallButtonSheet, new Vector2(453, 389), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29));
			loginButtons[0].OnClick += MainButtonPress;
			loginButtons[1].OnClick += MainButtonPress;

			//6 text boxes (by default) for creating a new account.
			for (int i = 0; i < accountCreateTextBoxes.Length; ++i)
			{
				//holy fuck! magic numbers!
				//basically, set the first  3 Y coord to start at 69  and move up by 51 each time
				//			 set the second 3 Y coord to start at 260 and move up by 51 each time
				int txtYCoord = (i < 3 ? 69 : 260) + (i < 3 ? i * 51 : (i - 3) * 51);
				XNATextBox txt = new XNATextBox(new Rectangle(358, txtYCoord, 240, textBoxTextures[0].Height), textBoxTextures, "Microsoft Sans Serif", 8.5f);

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
				accountCreateTextBoxes[i] = txt;
			}


			//create account / cancel
			Texture2D secondaryButtons = GFXLoader.TextureFromResource(GFXTypes.PreLoginUI, 14, true);
			createButtons[0] = new XNAButton(secondaryButtons, new Vector2(359, 417), new Rectangle(0, 0, 120, 40), new Rectangle(120, 0, 120, 40));
			createButtons[1] = new XNAButton(secondaryButtons, new Vector2(481, 417), new Rectangle(0, 40, 120, 40), new Rectangle(120, 40, 120, 40));
			createButtons[0].OnClick += MainButtonPress;
			createButtons[1].OnClick += MainButtonPress;

			passwordChangeBtn = new XNAButton(secondaryButtons, new Vector2(454, 417), new Rectangle(0, 120, 120, 40), new Rectangle(120, 120, 120, 40));
			passwordChangeBtn.OnClick += MainButtonPress;

			lblCredits = new XNALabel(new Rectangle(300, 260, 1, 1))
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

			lblVersionInfo = new XNALabel(new Rectangle(30, 457, 1, 1))
			{
				Text = string.Format("{0}.{1:000}.{2:000} - {3}:{4}", World.Instance.VersionMajor, World.Instance.VersionMinor, World.Instance.VersionClient, host, port),
				Font = new System.Drawing.Font("Microsoft Sans Serif", 7.0f),
				ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xb4, 0xa0, 0x8c)
			};

			//login/delete buttons for each character
			for (int i = 0; i < 3; ++i)
			{
				loginCharButtons[i] = new XNAButton(smallButtonSheet, new Vector2(495, 93 + i * 124), new Rectangle(0, 58, 91, 29), new Rectangle(91, 58, 91, 29));
				loginCharButtons[i].OnClick += CharModButtonPress;
				deleteCharButtons[i] = new XNAButton(smallButtonSheet, new Vector2(495, 121 + i * 124), new Rectangle(0, 87, 91, 29), new Rectangle(91, 87, 91, 29));
				deleteCharButtons[i].OnClick += CharModButtonPress;
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
				if (component != null && !mainButtons.Contains(component as XNAButton))
					component.Visible = false;
			}
			lblVersionInfo.Visible = true;

#if DEBUG
			//testinggame will login as testuser and login as the first character
			XNAButton testingame = new XNAButton(new Vector2(5, 5), "in-game");
			testingame.OnClick += (s, e) => new Thread(() =>
			{
				MainButtonPress(mainButtons[1], e); //press login
				Thread.Sleep(500);
				if (!World.Instance.Client.ConnectedAndInitialized)
					return;
				loginUsernameTextbox.Text = "testuser";
				loginPasswordTextbox.Text = "testuser";

				MainButtonPress(loginButtons[0], e); //login as acc testuser
				Thread.Sleep(500);
				CharModButtonPress(loginCharButtons[0], e); //login as char testuser
			}).Start();
#endif
		}

		//Pretty much controls how states transition between one another
		private void MainButtonPress(object sender, EventArgs e)
		{
			if (!IsActive)
				return;

			if (World.Instance.SoundEnabled && mainButtons.Contains(sender))
			{
				SoundManager.GetSoundEffectRef(SoundEffectID.ButtonClick).Play();
			}

			//switch on sender
			if (sender == mainButtons[0])
			{
				//try connect
				//if successful go to account creation state
				TryConnectToServer(() =>
				{
					doStateChange(GameStates.CreateAccount);

					EOScrollingDialog createAccountDlg = new EOScrollingDialog("");
					string message = World.Instance.DataFiles[World.Instance.Localized2].Data[(int)DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_1];
					message += "\n\n";
					message += World.Instance.DataFiles[World.Instance.Localized2].Data[(int)DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_2];
					message += "\n\n";
					message += World.Instance.DataFiles[World.Instance.Localized2].Data[(int)DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_3];
					createAccountDlg.MessageText = message;
				});
			}
			else if (sender == mainButtons[1])
			{
				//try connect
				//if successful go to account login state
				TryConnectToServer(() => doStateChange(GameStates.Login));
			}
			else if (sender == mainButtons[2])
			{
				doStateChange(GameStates.ViewCredits);
			}
			else if (sender == mainButtons[3])
			{
				if (World.Instance.Client.ConnectedAndInitialized)
					World.Instance.Client.Disconnect();
				Exit();
			}
			else if ((sender == backButton && currentState != GameStates.PlayingTheGame) || sender == createButtons[1] || sender == loginButtons[1])
			{
				Dispatcher.Subscriber = null;
				LostConnectionDialog();
				//disabled warning: in case I add code later below, need to remember that this should immediately return
// ReSharper disable once RedundantJumpStatement
				return;
			}
			else if (sender == backButton && currentState == GameStates.PlayingTheGame)
			{
				EODialog.Show(DATCONST1.EXIT_GAME_ARE_YOU_SURE, XNADialogButtons.OkCancel, EODialogStyle.SmallDialogSmallHeader, 
					(ss, ee) =>
					{
						if(ee.Result == XNADialogResult.OK)
						{
							Dispatcher.Subscriber = null;
							World.Instance.ResetGameElements();
							if (World.Instance.Client.ConnectedAndInitialized)
								World.Instance.Client.Disconnect();
							doStateChange(GameStates.Initial);
						}
					});
			}
			else if (sender == loginButtons[0])
			{
				if (loginUsernameTextbox.Text == "" || loginPasswordTextbox.Text == "")
					return;

				LoginReply reply;
				CharacterRenderData[] dataArray;
				if (!m_packetAPI.LoginRequest(loginUsernameTextbox.Text, loginPasswordTextbox.Text, out reply, out dataArray))
				{
					LostConnectionDialog();
					return;
				}

				if (reply != LoginReply.Ok)
				{
					EODialog.Show(m_packetAPI.LoginResponseMessage());
					return;
				}
				World.Instance.MainPlayer.SetAccountName(loginUsernameTextbox.Text);
				World.Instance.MainPlayer.ProcessCharacterData(dataArray);

				doStateChange(GameStates.LoggedIn);
			}
			else if (sender == createButtons[0])
			{
				switch (currentState)
				{
					case GameStates.CreateAccount:
					{
						if (accountCreateTextBoxes.Any(txt => txt.Text.Length == 0))
						{
							EODialog.Show(DATCONST1.ACCOUNT_CREATE_FIELDS_STILL_EMPTY);
							return;
						}

						if (accountCreateTextBoxes[0].Text.Length < 4)
						{
							EODialog.Show(DATCONST1.ACCOUNT_CREATE_NAME_TOO_SHORT);
							return;
						}

						if (accountCreateTextBoxes[0].Text.Distinct().Count() < 3)
						{
							EODialog.Show(DATCONST1.ACCOUNT_CREATE_NAME_TOO_OBVIOUS);
							return;
						}

						if (accountCreateTextBoxes[1].Text != accountCreateTextBoxes[2].Text)
						{
							EODialog.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);
							return;
						}

						if (accountCreateTextBoxes[1].Text.Length < 6)
						{
							EODialog.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);
							return;
						}

						if (accountCreateTextBoxes[1].Text.Distinct().Count() < 3)
						{
							EODialog.Show(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_OBVIOUS);
							return;
						}

						if (!System.Text.RegularExpressions.Regex.IsMatch(accountCreateTextBoxes[5].Text, //filter emails using regex
							@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b"))
						{
							EODialog.Show(DATCONST1.ACCOUNT_CREATE_EMAIL_INVALID);
							return;
						}

						AccountReply reply;
						if (!m_packetAPI.AccountCheckName(accountCreateTextBoxes[0].Text, out reply))
						{
							LostConnectionDialog();
							return;
						}

						if (reply != AccountReply.Continue)
						{
							EODialog.Show(m_packetAPI.AccountResponseMessage());
							return;
						}

						//show progress bar for account creation pending and THEN create the account
						string pbmessage = World.Instance.DataFiles[World.Instance.Localized1].Data[(int) DATCONST1.ACCOUNT_CREATE_ACCEPTED + 1];
						string pbcaption = World.Instance.DataFiles[World.Instance.Localized1].Data[(int) DATCONST1.ACCOUNT_CREATE_ACCEPTED];
						EOProgressDialog dlg = new EOProgressDialog(pbmessage, pbcaption);
						dlg.DialogClosing += (dlg_S, dlg_E) =>
						{
							if (dlg_E.Result != XNADialogResult.NO_BUTTON_PRESSED) return;

							if (!m_packetAPI.AccountCreate(accountCreateTextBoxes[0].Text,
								accountCreateTextBoxes[1].Text,
								accountCreateTextBoxes[3].Text,
								accountCreateTextBoxes[4].Text,
								accountCreateTextBoxes[5].Text,
								Config.GetHDDSerial(),
								out reply))
							{
								LostConnectionDialog();
								return;
							}

							DATCONST1 resource = m_packetAPI.AccountResponseMessage();
							if (reply != AccountReply.Created)
							{
								EODialog.Show(resource);
								return;
							}

							doStateChange(GameStates.Initial);
							EODialog.Show(resource);
						};

					}
						break;
					case GameStates.LoggedIn:
					{
						//Character_request: show create character dialog
						//Character_create: clicked ok in create character dialog
						CharacterReply reply;
						if (!m_packetAPI.CharacterRequest(out reply))
						{
							LostConnectionDialog();
							return;
						}

						if (reply != CharacterReply.Ok)
						{
							EODialog.Show("Server is not allowing you to create a character right now. This could be a bug.", "Server error");
							return;
						}

						EOCreateCharacterDialog createCharacter = new EOCreateCharacterDialog(textBoxTextures[3], Dispatcher);
						createCharacter.DialogClosing += (dlg_S, dlg_E) =>
						{
							if (dlg_E.Result != XNADialogResult.OK) return;

							CharacterRenderData[] dataArray;
							if (!m_packetAPI.CharacterCreate(createCharacter.Gender, createCharacter.HairType, createCharacter.HairColor, createCharacter.SkinColor, createCharacter.Name, out reply, out dataArray))
							{
								LostConnectionDialog();
								return;
							}

							if (reply != CharacterReply.Ok)
							{
								if (reply != CharacterReply.Full)
									dlg_E.CancelClose = true;
								EODialog.Show(m_packetAPI.CharacterResponseMessage());
								return;
							}

							EODialog.Show(DATCONST1.CHARACTER_CREATE_SUCCESS);
							World.Instance.MainPlayer.ProcessCharacterData(dataArray);
							doShowCharacters();
						};
					}
						break;
				}
			}
			else if (sender == passwordChangeBtn)
			{
				EOChangePasswordDialog dlg = new EOChangePasswordDialog(textBoxTextures[3], Dispatcher);
				dlg.DialogClosing += (dlg_S, dlg_E) =>
				{
					if (dlg_E.Result != XNADialogResult.OK) return;

					AccountReply reply;
					if (!m_packetAPI.AccountChangePassword(dlg.Username, dlg.OldPassword, dlg.NewPassword, out reply))
					{
						LostConnectionDialog();
						return;
					}

					EODialog.Show(m_packetAPI.AccountResponseMessage());

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
			if (loginCharButtons.Contains(sender))
			{
				index = loginCharButtons.ToList().FindIndex(x => x == sender);
				if (World.Instance.MainPlayer.CharData == null || World.Instance.MainPlayer.CharData.Length <= index)
					return;

				WelcomeRequestData data;
				if (!m_packetAPI.SelectCharacter(World.Instance.MainPlayer.CharData[index].id, out data))
				{
					LostConnectionDialog();
					return;
				}

				//handles the WelcomeRequestData object
				World.Instance.ApplyWelcomeRequest(m_packetAPI, data);

				//shows the connecting window
				EOConnectingDialog dlg = new EOConnectingDialog(m_packetAPI);
				dlg.DialogClosing += (dlgS, dlgE) =>
				{
					switch (dlgE.Result)
					{
						case XNADialogResult.OK:
							doStateChange(GameStates.PlayingTheGame);
							
							World.Instance.ApplyWelcomeMessage(dlg.WelcomeData);
							
							Hud = new HUD(this, m_packetAPI);
							Components.Add(Hud);
							Hud.SetNews(dlg.WelcomeData.News);
							Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.LOADING_GAME_HINT_FIRST);
							
							if(data.FirstTimePlayer)
								EODialog.Show(DATCONST1.WARNING_FIRST_TIME_PLAYERS, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
							break;
						case XNADialogResult.NO_BUTTON_PRESSED:
						{
							EODialog.Show(DATCONST1.CONNECTION_SERVER_BUSY);
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
					EODialog.Show("Character \'" + World.Instance.MainPlayer.CharData[index].name + "\' ", DATCONST1.CHARACTER_DELETE_FIRST_CHECK);
					charDeleteWarningShown = index;
					return;
				}

				//delete character at that index, if it exists
				int takeID;
				if (!m_packetAPI.CharacterTake(World.Instance.MainPlayer.CharData[index].id, out takeID))
				{
					LostConnectionDialog();
					return;
				}

				if (takeID != World.Instance.MainPlayer.CharData[index].id)
				{
					EODialog.Show("The server did not respond properly for deleting the character. Try again.", "Server error");
					return;
				}

				EODialog.Show("Character \'" + World.Instance.MainPlayer.CharData[index].name + "\' ",
					DATCONST1.CHARACTER_DELETE_CONFIRM, XNADialogButtons.OkCancel, EODialogStyle.SmallDialogLargeHeader,
					(dlgS, dlgE) =>
					{
						if (dlgE.Result == XNADialogResult.OK) //user clicked ok to delete their character. do the delete here.
						{
							CharacterRenderData[] dataArray;
							if (!m_packetAPI.CharacterRemove(World.Instance.MainPlayer.CharData[index].id, out dataArray))
							{
								LostConnectionDialog();
								return;
							}

							World.Instance.MainPlayer.ProcessCharacterData(dataArray);
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
			switch (currentState)
			{
				case GameStates.Login:
					if (sender == loginUsernameTextbox)
					{
						loginUsernameTextbox.Selected = false;
						Dispatcher.Subscriber = loginPasswordTextbox;
						loginPasswordTextbox.Selected = true;
					}
					else
					{
						loginUsernameTextbox.Selected = true;
						Dispatcher.Subscriber = loginUsernameTextbox;
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
							Dispatcher.Subscriber = accountCreateTextBoxes[next];
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
