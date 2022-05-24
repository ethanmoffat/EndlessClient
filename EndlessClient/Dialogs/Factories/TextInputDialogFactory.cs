using AutomaticTypeMapper;
using EndlessClient.Audio;
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
        private readonly ISfxPlayer _sfxPlayer;

        public TextInputDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                      IEODialogButtonService eoDialogButtonService,
                                      IKeyboardDispatcherRepository keyboardDispatcherRepository,
                                      IContentProvider contentProvider,
                                      ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _eoDialogButtonService = eoDialogButtonService;
            _keyboardDispatcherRepository = keyboardDispatcherRepository;
            _contentProvider = contentProvider;
            _sfxPlayer = sfxPlayer;
        }

        public TextInputDialog Create(string prompt, int maxInputChars = 12)
        {
            var dlg = new TextInputDialog(_nativeGraphicsManager,
                _eoDialogButtonService,
                _keyboardDispatcherRepository,
                _contentProvider,
                prompt,
                maxInputChars);
            dlg.DialogClosing += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);
            return dlg;
        }
    }

    public interface ITextInputDialogFactory
    {
        TextInputDialog Create(string prompt, int maxInputChars = 12);
    }
}
