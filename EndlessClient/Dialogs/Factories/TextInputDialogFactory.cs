using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class TextInputDialogFactory : ITextInputDialogFactory
    {
        private readonly IGameStateProvider _gameStateProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly IKeyboardDispatcherRepository _keyboardDispatcherRepository;
        private readonly IContentManagerProvider _contentManagerProvider;

        public TextInputDialogFactory(IGameStateProvider gameStateProvider,
                                      INativeGraphicsManager nativeGraphicsManager,
                                      IEODialogButtonService eoDialogButtonService,
                                      IKeyboardDispatcherRepository keyboardDispatcherRepository,
                                      IContentManagerProvider contentManagerProvider)
        {
            _gameStateProvider = gameStateProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _eoDialogButtonService = eoDialogButtonService;
            _keyboardDispatcherRepository = keyboardDispatcherRepository;
            _contentManagerProvider = contentManagerProvider;
        }

        public TextInputDialog Create(string prompt, int maxInputChars = 12)
        {
            return new TextInputDialog(_gameStateProvider,
                _nativeGraphicsManager,
                _eoDialogButtonService,
                _keyboardDispatcherRepository,
                _contentManagerProvider,
                prompt,
                maxInputChars);
        }
    }

    public interface ITextInputDialogFactory
    {
        TextInputDialog Create(string prompt, int maxInputChars = 12);
    }
}
