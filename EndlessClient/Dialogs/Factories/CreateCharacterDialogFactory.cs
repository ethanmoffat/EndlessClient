// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Rendering.Factories;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    public class CreateCharacterDialogFactory : ICreateCharacterDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IEODialogButtonService _dialogButtonService;

        public CreateCharacterDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                            IGameStateProvider gameStateProvider,
                                            ICharacterRendererFactory characterRendererFactory,
                                            IContentManagerProvider contentManagerProvider,
                                            IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                            IEOMessageBoxFactory eoMessageBoxFactory,
                                            IEODialogButtonService dialogButtonService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _characterRendererFactory = characterRendererFactory;
            _contentManagerProvider = contentManagerProvider;
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _dialogButtonService = dialogButtonService;
        }

        public CreateCharacterDialog BuildCreateCharacterDialog()
        {
            return new CreateCharacterDialog(_nativeGraphicsManager,
                _gameStateProvider,
                _characterRendererFactory,
                _contentManagerProvider.Content,
                _keyboardDispatcherProvider.Dispatcher,
                _eoMessageBoxFactory,
                _dialogButtonService);
        }
    }

    public interface ICreateCharacterDialogFactory
    {
        CreateCharacterDialog BuildCreateCharacterDialog();
    }
}