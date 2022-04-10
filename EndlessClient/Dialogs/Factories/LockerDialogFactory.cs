using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EndlessClient.HUD.Inventory;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class LockerDialogFactory : ILockerDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ILockerActions _lockerActions;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ICharacterProvider _characterProvider;
        private readonly ILockerDataProvider _lockerDataProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IEIFFileProvider _eifFileProvider;

        public LockerDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                   ILockerActions lockerActions,
                                   IEODialogButtonService dialogButtonService,
                                   ILocalizedStringFinder localizedStringFinder,
                                   IInventorySpaceValidator inventorySpaceValidator,
                                   IStatusLabelSetter statusLabelSetter,
                                   IEOMessageBoxFactory messageBoxFactory,
                                   ICharacterProvider characterProvider,
                                   ILockerDataProvider lockerDataProvider,
                                   IHudControlProvider hudControlProvider,
                                   IEIFFileProvider eifFileProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _lockerActions = lockerActions;
            _dialogButtonService = dialogButtonService;
            _localizedStringFinder = localizedStringFinder;
            _inventorySpaceValidator = inventorySpaceValidator;
            _statusLabelSetter = statusLabelSetter;
            _messageBoxFactory = messageBoxFactory;
            _characterProvider = characterProvider;
            _lockerDataProvider = lockerDataProvider;
            _hudControlProvider = hudControlProvider;
            _eifFileProvider = eifFileProvider;
        }

        public LockerDialog Create()
        {
            return new LockerDialog(_nativeGraphicsManager,
                                    _lockerActions,
                                    _dialogButtonService,
                                    _localizedStringFinder,
                                    _inventorySpaceValidator,
                                    _statusLabelSetter,
                                    _messageBoxFactory,
                                    _characterProvider,
                                    _lockerDataProvider,
                                    _hudControlProvider,
                                    _eifFileProvider);
        }
    }

    public interface ILockerDialogFactory
    {
        LockerDialog Create();
    }
}
