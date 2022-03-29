using AutomaticTypeMapper;
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

        public ItemTransferDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                         IEODialogButtonService eoDialogButtonService,
                                         ILocalizedStringFinder localizedStringFinder,
                                         IContentProvider contentProvider,
                                         IKeyboardDispatcherRepository keyboardDispatcherRepository)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _eoDialogButtonService = eoDialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _contentProvider = contentProvider;
            _keyboardDispatcherRepository = keyboardDispatcherRepository;
        }

        public ItemTransferDialog CreateItemTransferDialog(string itemName, ItemTransferDialog.TransferType transferType, int totalAmount, EOResourceID message)
        {
            return new ItemTransferDialog(_nativeGraphicsManager,
                _eoDialogButtonService,
                _localizedStringFinder,
                _contentProvider,
                _keyboardDispatcherRepository,
                itemName,
                transferType,
                totalAmount,
                message);
        }
    }

    public interface IItemTransferDialogFactory
    {
        ItemTransferDialog CreateItemTransferDialog(string itemName, ItemTransferDialog.TransferType transferType, int totalAmount, EOResourceID message);
    }
}
