// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Game;
using EOLib.Graphics;

namespace EndlessClient.Controls.ControlSets
{
	public class ControlSetFactory : IControlSetFactory
	{
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IContentManagerProvider _contentManagerProvider;
		private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;

		public ControlSetFactory(INativeGraphicsManager nativeGraphicsManager,
										  IContentManagerProvider contentManagerProvider,
										  IKeyboardDispatcherProvider keyboardDispatcherProvider)
		{
			_nativeGraphicsManager = nativeGraphicsManager;
			_contentManagerProvider = contentManagerProvider;
			_keyboardDispatcherProvider = keyboardDispatcherProvider;
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
				case GameStates.Initial: return new InitialControlSet();
				case GameStates.CreateAccount: return new CreateAccountControlSet(_keyboardDispatcherProvider.Dispatcher);
				case GameStates.Login: return new LoginPromptControlSet(_keyboardDispatcherProvider.Dispatcher);
				case GameStates.ViewCredits: return new ViewCreditsControlSet();
				default: throw new ArgumentOutOfRangeException("newState", newState, null);
			}
		}
	}
}
