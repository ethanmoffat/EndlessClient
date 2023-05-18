using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EndlessClient.HUD.Inventory;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class ChestDialogFactory : IChestDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IChestActions _chestActions;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IMapItemGraphicProvider _mapItemGraphicProvider;
        private readonly IChestDataProvider _chestDataProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICharacterProvider _characterProvider;

        public ChestDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                  IChestActions chestActions,
                                  IEOMessageBoxFactory messageBoxFactory,
                                  IEODialogButtonService dialogButtonService,
                                  IStatusLabelSetter statusLabelSetter,
                                  ILocalizedStringFinder localizedStringFinder,
                                  IInventorySpaceValidator inventorySpaceValidator,
                                  IMapItemGraphicProvider mapItemGraphicProvider,
                                  IChestDataProvider chestDataProvider,
                                  IEIFFileProvider eifFileProvider,
                                  ICharacterProvider characterProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _chestActions = chestActions;
            _messageBoxFactory = messageBoxFactory;
            _dialogButtonService = dialogButtonService;
            _statusLabelSetter = statusLabelSetter;
            _localizedStringFinder = localizedStringFinder;
            _inventorySpaceValidator = inventorySpaceValidator;
            _mapItemGraphicProvider = mapItemGraphicProvider;
            _chestDataProvider = chestDataProvider;
            _eifFileProvider = eifFileProvider;
            _characterProvider = characterProvider;
        }

        public ChestDialog Create()
        {
            return new ChestDialog(_nativeGraphicsManager,
                                   _chestActions,
                                   _messageBoxFactory,
                                   _dialogButtonService,
                                   _statusLabelSetter,
                                   _localizedStringFinder,
                                   _inventorySpaceValidator,
                                   _mapItemGraphicProvider,
                                   _chestDataProvider,
                                   _eifFileProvider,
                                   _characterProvider);
        }
    }

    public interface IChestDialogFactory
    {
        ChestDialog Create();
    }
}
