using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.Input;
using EOLib.Graphics;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class ItemTransferDialogFactory : IItemTransferDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IContentProvider _contentProvider;
        private readonly IKeyboardDispatcherRepository _keyboardDispatcherRepository;
        private readonly ISfxPlayer _sfxPlayer;

        public ItemTransferDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                         IEODialogButtonService eoDialogButtonService,
                                         ILocalizedStringFinder localizedStringFinder,
                                         IContentProvider contentProvider,
                                         IKeyboardDispatcherRepository keyboardDispatcherRepository,
                                         ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _eoDialogButtonService = eoDialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _contentProvider = contentProvider;
            _keyboardDispatcherRepository = keyboardDispatcherRepository;
            _sfxPlayer = sfxPlayer;
        }

        public ItemTransferDialog CreateItemTransferDialog(string itemName, ItemTransferDialog.TransferType transferType, int totalAmount, EOResourceID message)
        {
            var dlg = new ItemTransferDialog(_nativeGraphicsManager,
                _eoDialogButtonService,
                _localizedStringFinder,
                _contentProvider,
                _keyboardDispatcherRepository,
                itemName,
                transferType,
                totalAmount,
                message);
            dlg.DialogClosing += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);
            return dlg;
        }
    }

    public interface IItemTransferDialogFactory
    {
        ItemTransferDialog CreateItemTransferDialog(string itemName, ItemTransferDialog.TransferType transferType, int totalAmount, EOResourceID message);
    }
}
