// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Factories;
using EndlessClient.UIControls;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
	public class CreateCharacterDialogFactory : ICreateCharacterDialogFactory
	{
		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
		private readonly IGameStateProvider _gameStateProvider;
		private readonly ICharacterRendererFactory _characterRendererFactory;
		private readonly IContentManagerProvider _contentManagerProvider;
		private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
		private readonly IEOMessageBoxFactory _eoMessageBoxFactory;

		public CreateCharacterDialogFactory(INativeGraphicsManager nativeGraphicsManager,
											IGraphicsDeviceProvider graphicsDeviceProvider,
											IGameStateProvider gameStateProvider,
											ICharacterRendererFactory characterRendererFactory,
											IContentManagerProvider contentManagerProvider,
											IKeyboardDispatcherProvider keyboardDispatcherProvider,
											IEOMessageBoxFactory eoMessageBoxFactory)
		{
			_nativeGraphicsManager = nativeGraphicsManager;
			_graphicsDeviceProvider = graphicsDeviceProvider;
			_gameStateProvider = gameStateProvider;
			_characterRendererFactory = characterRendererFactory;
			_contentManagerProvider = contentManagerProvider;
			_keyboardDispatcherProvider = keyboardDispatcherProvider;
			_eoMessageBoxFactory = eoMessageBoxFactory;
		}

		public CreateCharacterDialog BuildCreateCharacterDialog()
		{
			return new CreateCharacterDialog(_nativeGraphicsManager,
				_graphicsDeviceProvider,
				_gameStateProvider,
				_characterRendererFactory,
				_contentManagerProvider.Content,
				_keyboardDispatcherProvider.Dispatcher,
				_eoMessageBoxFactory);
		}
	}
}