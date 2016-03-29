// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace EndlessClient.Controls.ControlSets
{
	public class LoginPromptControlSet : BaseGameStateControlSet, IGameStateControlSet
	{
		private IGameComponent _createAccount,
							   _login,
							   _viewCredits,
							   _exitGame,
							   _versionInfo;

		private IGameComponent _tbLogin,
							   _tbPassword,
							   _btnLogin,
							   _btnCancel;

		//todo: add some sort of picturebox control for person 1 and login panel background

		public GameStates GameState { get { return GameStates.Login; } }

		public override void InitializeControls(IGameStateControlSet currentControlSet)
		{
			_createAccount = GetControl(currentControlSet, GameControlIdentifier.InitialCreateAccount, GetMainCreateAccountButton);
			_login = GetControl(currentControlSet, GameControlIdentifier.InitialLogin, GetMainLoginButton);
			_viewCredits = GetControl(currentControlSet, GameControlIdentifier.InitialViewCredits, GetViewCreditsButton);
			_exitGame = GetControl(currentControlSet, GameControlIdentifier.InitialExitGame, GetExitButton);
			_versionInfo = GetControl(currentControlSet, GameControlIdentifier.InitialVersionLabel, GetVersionInfoLabel);

			_tbLogin = GetControl(currentControlSet, GameControlIdentifier.LoginAccountName, GetLoginUserNameTextBox);
			_tbPassword = GetControl(currentControlSet, GameControlIdentifier.LoginPassword, GetLoginPasswordTextBox);
			_btnLogin = GetControl(currentControlSet, GameControlIdentifier.LoginButton, GetLoginAccountButton);
			_btnCancel = GetControl(currentControlSet, GameControlIdentifier.LoginCancel, GetLoginCancelButton);

			_allComponents.Add(_createAccount);
			_allComponents.Add(_login);
			_allComponents.Add(_viewCredits);
			_allComponents.Add(_exitGame);
			_allComponents.Add(_versionInfo);

			_allComponents.Add(_tbLogin);
			_allComponents.Add(_tbPassword);
			_allComponents.Add(_btnLogin);
			_allComponents.Add(_btnCancel);
		}

		public IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.InitialCreateAccount: return _createAccount;
				case GameControlIdentifier.InitialLogin: return _login;
				case GameControlIdentifier.InitialViewCredits: return _viewCredits;
				case GameControlIdentifier.InitialExitGame: return _exitGame;
				case GameControlIdentifier.InitialVersionLabel: return _versionInfo;
				case GameControlIdentifier.LoginAccountName: return _tbLogin;
				case GameControlIdentifier.LoginPassword: return _tbPassword;
				case GameControlIdentifier.LoginButton: return _btnLogin;
				case GameControlIdentifier.LoginCancel: return _btnCancel;
				default: return null;
			}
		}
	}
}
