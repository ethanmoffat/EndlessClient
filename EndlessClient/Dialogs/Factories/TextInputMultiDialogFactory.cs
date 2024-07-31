using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD.Chat;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class TextMultiInputDialogFactory : ITextMultiInputDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly IContentProvider _contentProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public TextMultiInputDialogFactory(INativeGraphicsManager nativeGraphicsManager,
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

        public TextMultiInputDialog Create(string title, string prompt, TextMultiInputDialog.DialogSize size, params TextMultiInputDialog.InputInfo[] inputInfo)
        {
            var dlg = new TextMultiInputDialog(_nativeGraphicsManager,
                _chatTextBoxActions,
                _eoDialogButtonService,
                _contentProvider,
                size,
                title,
                prompt,
                inputInfo);
            dlg.DialogClosing += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);
            return dlg;
        }
    }

    public interface ITextMultiInputDialogFactory
    {
        TextMultiInputDialog Create(string title,
                                    string prompt,
                                    TextMultiInputDialog.DialogSize size,
                                    params TextMultiInputDialog.InputInfo[] inputInfo);
    }
}