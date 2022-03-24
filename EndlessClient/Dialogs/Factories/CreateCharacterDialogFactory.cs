using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Rendering.Factories;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(ICreateCharacterDialogFactory))]
    public class CreateCharacterDialogFactory : ICreateCharacterDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IContentProvider _contentProvider;
        private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IEODialogButtonService _dialogButtonService;

        public CreateCharacterDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                            IGameStateProvider gameStateProvider,
                                            ICharacterRendererFactory characterRendererFactory,
                                            IContentProvider contentProvider,
                                            IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                            IEOMessageBoxFactory eoMessageBoxFactory,
                                            IEODialogButtonService dialogButtonService)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _characterRendererFactory = characterRendererFactory;
            _contentProvider = contentProvider;
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _dialogButtonService = dialogButtonService;
        }

        public CreateCharacterDialog BuildCreateCharacterDialog()
        {
            return new CreateCharacterDialog(_nativeGraphicsManager,
                _gameStateProvider,
                _characterRendererFactory,
                _contentProvider,
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