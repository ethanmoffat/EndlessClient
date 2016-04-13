// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class InitialControlSet : BaseControlSet
	{
		private readonly IConfigurationProvider _configProvider;

		private IGameComponent _createAccount,
							   _login,
							   _viewCredits,
							   _exitGame,
							   _versionInfo;

		//todo: add some sort of picture box control for person one picture

		public override GameStates GameState { get { return GameStates.Initial; } }

		public InitialControlSet(IConfigurationProvider configProvider)
		{
			_configProvider = configProvider;
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			_createAccount = GetControl(currentControlSet, GameControlIdentifier.InitialCreateAccount, GetMainCreateAccountButton);
			_login = GetControl(currentControlSet, GameControlIdentifier.InitialLogin, GetMainLoginButton);
			_viewCredits = GetControl(currentControlSet, GameControlIdentifier.InitialViewCredits, GetViewCreditsButton);
			_exitGame = GetControl(currentControlSet, GameControlIdentifier.InitialExitGame, GetExitButton);
			_versionInfo = GetControl(currentControlSet, GameControlIdentifier.InitialVersionLabel, GetVersionInfoLabel);

			_allComponents.Add(_createAccount);
			_allComponents.Add(_login);
			_allComponents.Add(_viewCredits);
			_allComponents.Add(_exitGame);
			_allComponents.Add(_versionInfo);
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.InitialCreateAccount: return _createAccount;
				case GameControlIdentifier.InitialLogin: return _login;
				case GameControlIdentifier.InitialViewCredits: return _viewCredits;
				case GameControlIdentifier.InitialExitGame: return _exitGame;
				case GameControlIdentifier.InitialVersionLabel: return _versionInfo;
				default: return null;
			}
		}

		private XNAButton GetMainCreateAccountButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialCreateAccount);
		}

		private XNAButton GetMainLoginButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialLogin);
		}

		private XNAButton GetViewCreditsButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialViewCredits);
		}

		private XNAButton GetExitButton()
		{
			return MainButtonCreationHelper(GameControlIdentifier.InitialExitGame);
		}

		private XNAButton MainButtonCreationHelper(GameControlIdentifier whichControl)
		{
			int i;
			switch (whichControl)
			{
				case GameControlIdentifier.InitialCreateAccount: i = 0; break;
				case GameControlIdentifier.InitialLogin: i = 1; break;
				case GameControlIdentifier.InitialViewCredits: i = 2; break;
				case GameControlIdentifier.InitialExitGame: i = 3; break;
				default: throw new ArgumentException("Invalid control specified for helper", "whichControl");
			}

			var widthFactor = _mainButtonTexture.Width / 2;
			var heightFactor = _mainButtonTexture.Height / 4;
			var outSource = new Rectangle(0, i * heightFactor, widthFactor, heightFactor);
			var overSource = new Rectangle(widthFactor, i * heightFactor, widthFactor, heightFactor);

			return new XNAButton(_mainButtonTexture, new Vector2(26, 278 + i * 40), outSource, overSource);
		}

		private XNALabel GetVersionInfoLabel()
		{
			return new XNALabel(new Rectangle(25, 453, 1, 1), Constants.FontSize07)
			{
				Text = string.Format(Constants.VersionInfoFormat,
									 _configProvider.VersionMajor,
									 _configProvider.VersionMinor,
									 _configProvider.VersionBuild,
									 _configProvider.Host,
									 _configProvider.Port),
				ForeColor = Constants.BeigeText
			};
		}
	}
}
