// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
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
		private readonly GraphicsDeviceManager _graphicsDeviceManager;
		private SpriteBatch _spriteBatch;

		public KeyboardDispatcher Dispatcher { get; private set; }

		private readonly XNAButton[] _loginCharButtons = new XNAButton[3];

		private XNAButton _backButton;
		private bool _backButtonPressed; //workaround so the lost connection dialog doesn't show from the client disconnect event

		public HUD.HUD Hud { get; private set; }
		public SoundManager SoundManager { get; private set; }

		private void InitializeControls(bool reinit = false)
		{
			Texture2D back = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 24, true);
			_backButton = new XNAButton(back, new Vector2(589, 0), new Rectangle(0, 0, back.Width, back.Height / 2),
				new Rectangle(0, back.Height / 2, back.Width, back.Height / 2)) { DrawOrder = 100 };
			_backButton.OnClick += MainButtonPress;
			_backButton.ClickArea = new Rectangle(4, 16, 16, 16);

			Texture2D smallButtonSheet = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);

			//login buttons for each character
			for (int i = 0; i < 3; ++i)
			{
				_loginCharButtons[i] = new XNAButton(smallButtonSheet, new Vector2(495, 93 + i * 124), new Rectangle(0, 58, 91, 29), new Rectangle(91, 58, 91, 29));
				_loginCharButtons[i].OnClick += CharModButtonPress;
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
				if (component != null)
					component.Visible = false;
			}
		}

		private void MainButtonPress(object sender, EventArgs e)
		{
			if (!IsActive)
				return;
			
			if (sender == _backButton && State == GameStates.PlayingTheGame)
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
		}

		private void CharModButtonPress(object sender, EventArgs e)
		{
			//click login: send WELCOME_REQUEST, get WELCOME_REPLY
			//Send WELCOME_AGREE for map/pubs if needed
			//Send WELCOME_MSG, get WELCOME_REPLY
			//log in if all okay

			if (_loginCharButtons.Contains(sender))
			{
				var index = _loginCharButtons.ToList().FindIndex(x => x == sender);
				if (OldWorld.Instance.MainPlayer.CharData == null || OldWorld.Instance.MainPlayer.CharData.Length <= index)
					return;

				WelcomeRequestData data;
				if (!API.SelectCharacter(OldWorld.Instance.MainPlayer.CharData[index].id, out data))
				{
					DoShowLostConnectionDialogAndReturnToMainMenu();
					return;
				}

				//handles the WelcomeRequestData object
				OldWorld.Instance.ApplyWelcomeRequest(API, data);

				//shows the connecting window
				GameLoadingDialog dlg = new GameLoadingDialog(API);
				dlg.DialogClosing += (dlgS, dlgE) =>
				{
					switch (dlgE.Result)
					{
						case XNADialogResult.OK:
							doStateChange(GameStates.PlayingTheGame);
							
							OldWorld.Instance.ApplyWelcomeMessage(dlg.WelcomeData);

							Hud = new HUD.HUD(this, API);
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
		}
	}
}
