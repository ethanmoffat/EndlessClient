using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Inventory;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Shop;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class ShopDialogFactory : IShopDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IItemTransferDialogFactory _itemTransferDialogFactory;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IShopDataProvider _shopDataProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;

        public ShopDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                 IEOMessageBoxFactory messageBoxFactory,
                                 IItemTransferDialogFactory itemTransferDialogFactory,
                                 IEODialogButtonService dialogButtonService,
                                 IEODialogIconService dialogIconService,
                                 ILocalizedStringFinder localizedStringFinder,
                                 IShopDataProvider shopDataProvider,
                                 ICharacterInventoryProvider characterInventoryProvider,
                                 IEIFFileProvider eifFileProvider,
                                 IInventorySpaceValidator inventorySpaceValidator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _messageBoxFactory = messageBoxFactory;
            _itemTransferDialogFactory = itemTransferDialogFactory;
            _dialogButtonService = dialogButtonService;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _shopDataProvider = shopDataProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
            _inventorySpaceValidator = inventorySpaceValidator;
        }

        public ShopDialog Create()
        {
            return new ShopDialog(_nativeGraphicsManager,
                _messageBoxFactory,
                _itemTransferDialogFactory,
                _dialogButtonService,
                _dialogIconService,
                _localizedStringFinder,
                _shopDataProvider,
                _characterInventoryProvider,
                _eifFileProvider,
                _inventorySpaceValidator);
        }
    }

    public interface IShopDialogFactory
    {
        ShopDialog Create();
    }
}
