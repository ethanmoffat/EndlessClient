// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Content;
using EndlessClient.GameExecution;
using EndlessClient.Controllers;
using EndlessClient.UIControls;
using EOLib.Graphics;
using EOLib.IO.Repositories;

namespace EndlessClient.ControlSets
{
	public class ControlSetFactory : IControlSetFactory
	{
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IContentManagerProvider _contentManagerProvider;
		private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
		private readonly IConfigurationProvider _configProvider;
		private readonly IMainButtonControllerProvider _mainButtonControllerProvider;

		public ControlSetFactory(INativeGraphicsManager nativeGraphicsManager,
								 IContentManagerProvider contentManagerProvider,
								 IKeyboardDispatcherProvider keyboardDispatcherProvider,
								 IConfigurationProvider configProvider,
								 IMainButtonControllerProvider mainButtonControllerProvider)
		{
			_nativeGraphicsManager = nativeGraphicsManager;
			_contentManagerProvider = contentManagerProvider;
			_keyboardDispatcherProvider = keyboardDispatcherProvider;
			_configProvider = configProvider;
			_mainButtonControllerProvider = mainButtonControllerProvider;
		}

		public IControlSet CreateControlsForState(GameStates newState, IControlSet currentControlSet)
		{
			var controlSet = GetSetBasedOnState(newState);
			controlSet.InitializeResources(_nativeGraphicsManager, _contentManagerProvider.Content);
			controlSet.InitializeControls(currentControlSet);
			return controlSet;
		}

		private IControlSet GetSetBasedOnState(GameStates newState)
		{
			switch (newState)
			{
				case GameStates.Initial: return new InitialControlSet(_configProvider, MainButtonController);
				case GameStates.CreateAccount: return new CreateAccountControlSet(_keyboardDispatcherProvider.Dispatcher, MainButtonController);
				case GameStates.Login: return new LoginPromptControlSet(_keyboardDispatcherProvider.Dispatcher, _configProvider, MainButtonController);
				case GameStates.ViewCredits: return new ViewCreditsControlSet(_configProvider, MainButtonController);
				default: throw new ArgumentOutOfRangeException("newState", newState, null);
			}
		}

		private IMainButtonController MainButtonController
		{
			get { return _mainButtonControllerProvider.MainButtonController; }
		}
	}
}
