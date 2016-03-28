// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Controls.ControlSets
{
	public class InitialGameStateControlSet : BaseGameStateControlSet, IGameStateControlSet
	{
		private readonly IGameStateControlSet _currentControlSet;
		private readonly List<IGameComponent> _allComponents;

		private IGameComponent _createAccount,
							   _login,
							   _viewCredits,
							   _exitGame,
							   _versionInfo;

		public GameStates GameState { get { return GameStates.Initial; } }

		public IReadOnlyList<IGameComponent> AllComponents
		{
			get { return _allComponents; }
		}

		public IReadOnlyList<XNAControl> XNAControlComponents
		{
			get { return _allComponents.OfType<XNAControl>().ToList(); }
		}

		public InitialGameStateControlSet(INativeGraphicsManager gfxManager, 
										  IGameStateControlSet currentControlSet)
			: base(gfxManager)
		{
			_currentControlSet = currentControlSet;
			_allComponents = new List<IGameComponent>(5);
		}

		public void CreateControls()
		{
			_createAccount = _currentControlSet.FindComponentByControlIdentifier(GameControlIdentifier.InitialCreateAccount) ??
			                 GetCreateAccountButton();
			_login         = _currentControlSet.FindComponentByControlIdentifier(GameControlIdentifier.InitialLogin) ??
			                 GetLoginButton();
			_viewCredits   = _currentControlSet.FindComponentByControlIdentifier(GameControlIdentifier.InitialViewCredits) ??
			                 GetViewCreditsButton();
			_exitGame      = _currentControlSet.FindComponentByControlIdentifier(GameControlIdentifier.InitialExitGame) ??
			                 GetExitButton();
			_versionInfo   = _currentControlSet.FindComponentByControlIdentifier(GameControlIdentifier.InitialVersionLabel) ??
			                 GetVersionInfoLabel();

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
