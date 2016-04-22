// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Controllers;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Login;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class LoginPromptControlSet : InitialControlSet
	{
		private readonly KeyboardDispatcher _dispatcher;
		private readonly IMainButtonController _mainButtonController;
		private readonly ILoginController _loginController;

		private XNATextBox _tbUsername, _tbPassword;
		private XNAButton _btnLogin, _btnCancel;
		private PictureBox _loginPanelBackground;

		private TextBoxClickEventHandler _clickHandler;
		private TextBoxTabEventHandler _tabHandler;
		private Texture2D _loginBackgroundTexture;

		public override GameStates GameState { get { return GameStates.Login; } }

		public LoginPromptControlSet(KeyboardDispatcher dispatcher,
									 IConfigurationProvider configProvider,
									 IMainButtonController mainButtonController,
									 ILoginController loginController)
			: base(configProvider, mainButtonController)
		{
			_dispatcher = dispatcher;
			_mainButtonController = mainButtonController;
			_loginController = loginController;
		}

		public override void InitializeResources(INativeGraphicsManager gfxManager, ContentManager xnaContentManager)
		{
			base.InitializeResources(gfxManager, xnaContentManager);

			_loginBackgroundTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 2);
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			base.InitializeControlsHelper(currentControlSet);

			_loginPanelBackground = GetControl(currentControlSet, GameControlIdentifier.LoginPanelBackground, GetLoginPanelBackground);
			_tbUsername = GetControl(currentControlSet, GameControlIdentifier.LoginAccountName, GetLoginUserNameTextBox);
			_tbPassword = GetControl(currentControlSet, GameControlIdentifier.LoginPassword, GetLoginPasswordTextBox);
			_btnLogin = GetControl(currentControlSet, GameControlIdentifier.LoginButton, GetLoginAccountButton);
			_btnCancel = GetControl(currentControlSet, GameControlIdentifier.LoginCancel, GetLoginCancelButton);

			_allComponents.Add(_loginPanelBackground);
			_allComponents.Add(_tbUsername);
			_allComponents.Add(_tbPassword);
			_allComponents.Add(_btnLogin);
			_allComponents.Add(_btnCancel);

			_clickHandler = new TextBoxClickEventHandler(_dispatcher, _allComponents.OfType<XNATextBox>().ToArray());
			_tabHandler = new TextBoxTabEventHandler(_dispatcher, _allComponents.OfType<XNATextBox>().ToArray());

			if (_dispatcher.Subscriber != null)
				_dispatcher.Subscriber.Selected = false;
			_dispatcher.Subscriber = _tbUsername;
			_dispatcher.Subscriber.Selected = true;
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.LoginPanelBackground: return _loginPanelBackground;
				case GameControlIdentifier.LoginAccountName: return _tbUsername;
				case GameControlIdentifier.LoginPassword: return _tbPassword;
				case GameControlIdentifier.LoginButton: return _btnLogin;
				case GameControlIdentifier.LoginCancel: return _btnCancel;
				default: return base.FindComponentByControlIdentifier(control);
			}
		}

		private PictureBox GetLoginPanelBackground()
		{
			return new PictureBox(_loginBackgroundTexture)
			{
				DrawLocation = new Vector2(266, 291),
				DrawOrder = _personPicture.DrawOrder + 1
			};
		}

		private XNATextBox GetLoginUserNameTextBox()
		{
			return new XNATextBox(new Rectangle(402, 322, 140, _textBoxTextures[0].Height), _textBoxTextures, Constants.FontSize08)
			{
				MaxChars = 16,
				DefaultText = "Username",
				LeftPadding = 4,
				DrawOrder = _personPicture.DrawOrder + 2
			};
		}

		private XNATextBox GetLoginPasswordTextBox()
		{
			return new XNATextBox(new Rectangle(402, 358, 140, _textBoxTextures[0].Height), _textBoxTextures, Constants.FontSize08)
			{
				MaxChars = 12,
				PasswordBox = true,
				LeftPadding = 4,
				DefaultText = "Password",
				DrawOrder = _personPicture.DrawOrder + 2
			};
		}

		private XNAButton GetLoginAccountButton()
		{
			var button = new XNAButton(_smallButtonSheet, new Vector2(361, 389), new Rectangle(0, 0, 91, 29), new Rectangle(91, 0, 91, 29))
			{
				DrawOrder = _personPicture.DrawOrder + 2
			};
			button.OnClick += DoLogin;
			return button;
		}

		private async void DoLogin(object sender, EventArgs e)
		{
			var loginParameters = new LoginParameters(_tbUsername.Text, _tbPassword.Text);
			await _loginController.LoginToAccount(loginParameters);
		}

		private XNAButton GetLoginCancelButton()
		{
			var button = new XNAButton(_smallButtonSheet, new Vector2(453, 389), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29))
			{
				DrawOrder = _personPicture.DrawOrder + 2
			};
			button.OnClick += (o, e) => _mainButtonController.GoToInitialState();
			return button;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_clickHandler.Dispose();
				_tabHandler.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
