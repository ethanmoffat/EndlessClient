// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Controls.ControlSets
{
	public class InitialGameStateControlSet : BaseGameStateControlSet, IGameStateControlSet
	{
		private IGameComponent _createAccount,
							   _login,
							   _viewCredits,
							   _exitGame,
							   _versionInfo;

		public GameStates GameState { get { return GameStates.Initial; } }

		public InitialGameStateControlSet(INativeGraphicsManager gfxManager)
			: base(gfxManager) { }

		public override void CreateControls(IGameStateControlSet _currentControlSet)
		{
			_createAccount = GetControl(_currentControlSet, GameControlIdentifier.InitialCreateAccount, GetCreateAccountButton);
			_login = GetControl(_currentControlSet, GameControlIdentifier.InitialLogin, GetLoginButton);
			_viewCredits = GetControl(_currentControlSet, GameControlIdentifier.InitialViewCredits, GetViewCreditsButton);
			_exitGame = GetControl(_currentControlSet, GameControlIdentifier.InitialExitGame, GetExitButton);
			_versionInfo = GetControl(_currentControlSet, GameControlIdentifier.InitialVersionLabel, GetVersionInfoLabel);

			_allComponents.Add(_createAccount);
			_allComponents.Add(_login);
			_allComponents.Add(_viewCredits);
			_allComponents.Add(_exitGame);
			_allComponents.Add(_versionInfo);
		}

		public IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.InitialCreateAccount:
					return _createAccount;
				case GameControlIdentifier.InitialLogin:
					return _login;
				case GameControlIdentifier.InitialViewCredits:
					return _viewCredits;
				case GameControlIdentifier.InitialExitGame:
					return _exitGame;
				case GameControlIdentifier.InitialVersionLabel:
					return _versionInfo;
				default:
					return null;
			}
		}
	}
}
