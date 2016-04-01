// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EndlessClient.Game;
using EndlessClient.Input;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class LoginPromptControlSet : InitialControlSet
	{
		private readonly KeyboardDispatcher _dispatcher;

		private IGameComponent _tbLogin,
							   _tbPassword,
							   _btnLogin,
							   _btnCancel;

		private TextBoxClickEventHandler _clickHandler;
		private TextBoxTabEventHandler _tabHandler;

		//todo: add some sort of picturebox control for login panel background

		public override GameStates GameState { get { return GameStates.Login; } }

		public LoginPromptControlSet(KeyboardDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			base.InitializeControlsHelper(currentControlSet);

			_tbLogin = GetControl(currentControlSet, GameControlIdentifier.LoginAccountName, GetLoginUserNameTextBox);
			_tbPassword = GetControl(currentControlSet, GameControlIdentifier.LoginPassword, GetLoginPasswordTextBox);
			_btnLogin = GetControl(currentControlSet, GameControlIdentifier.LoginButton, GetLoginAccountButton);
			_btnCancel = GetControl(currentControlSet, GameControlIdentifier.LoginCancel, GetLoginCancelButton);

			_allComponents.Add(_tbLogin);
			_allComponents.Add(_tbPassword);
			_allComponents.Add(_btnLogin);
			_allComponents.Add(_btnCancel);

			_clickHandler = new TextBoxClickEventHandler(_dispatcher, _allComponents.OfType<XNATextBox>().ToArray());
			_tabHandler = new TextBoxTabEventHandler(_dispatcher, _allComponents.OfType<XNATextBox>().ToArray());
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.LoginPanelBackground: return null;
				case GameControlIdentifier.LoginAccountName: return _tbLogin;
				case GameControlIdentifier.LoginPassword: return _tbPassword;
				case GameControlIdentifier.LoginButton: return _btnLogin;
				case GameControlIdentifier.LoginCancel: return _btnCancel;
				default: return base.FindComponentByControlIdentifier(control);
			}
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
