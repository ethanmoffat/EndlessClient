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
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly IKeyboardDispatcherRepository _keyboardDispatcherRepository;
        private readonly IContentProvider _contentProvider;

        public TextInputDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                      IEODialogButtonService eoDialogButtonService,
                                      IKeyboardDispatcherRepository keyboardDispatcherRepository,
                                      IContentProvider contentProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _eoDialogButtonService = eoDialogButtonService;
            _keyboardDispatcherRepository = keyboardDispatcherRepository;
            _contentProvider = contentProvider;
        }

        public TextInputDialog Create(string prompt, int maxInputChars = 12)
        {
            return new TextInputDialog(_nativeGraphicsManager,
                _eoDialogButtonService,
                _keyboardDispatcherRepository,
                _contentProvider,
                prompt,
                maxInputChars);
        }
    }

    public interface ITextInputDialogFactory
    {
        TextInputDialog Create(string prompt, int maxInputChars = 12);
    }
}
