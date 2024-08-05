using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD.Chat;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class TextInputDialogFactory : ITextInputDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly IContentProvider _contentProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public TextInputDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                      IChatTextBoxActions chatTextBoxActions,
                                      IEODialogButtonService eoDialogButtonService,
                                      IContentProvider contentProvider,
                                      ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _chatTextBoxActions = chatTextBoxActions;
            _eoDialogButtonService = eoDialogButtonService;
            _contentProvider = contentProvider;
            _sfxPlayer = sfxPlayer;
        }

        public TextInputDialog Create(string prompt, int maxInputChars = 12)
        {
            var dlg = new TextInputDialog(_nativeGraphicsManager,
                _chatTextBoxActions,
                _eoDialogButtonService,
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