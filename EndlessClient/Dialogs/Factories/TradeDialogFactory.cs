using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EndlessClient.HUD.Inventory;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Trade;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class TradeDialogFactory : ITradeDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ITradeActions _tradeActions;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly ITradeProvider _tradeProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IMapItemGraphicProvider _mapItemGraphicProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public TradeDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                  ITradeActions tradeActions,
                                  ILocalizedStringFinder localizedStringFinder,
                                  IEODialogButtonService dialogButtonService,
                                  IEOMessageBoxFactory messageBoxFactory,
                                  IStatusLabelSetter statusLabelSetter,
                                  IInventorySpaceValidator inventorySpaceValidator,
                                  ITradeProvider tradeProvider,
                                  ICharacterProvider characterProvider,
                                  IEIFFileProvider eifFileProvider,
                                  IMapItemGraphicProvider mapItemGraphicProvider,
                                  ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _tradeActions = tradeActions;
            _localizedStringFinder = localizedStringFinder;
            _dialogButtonService = dialogButtonService;
            _messageBoxFactory = messageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _inventorySpaceValidator = inventorySpaceValidator;
            _tradeProvider = tradeProvider;
            _characterProvider = characterProvider;
            _eifFileProvider = eifFileProvider;
            _mapItemGraphicProvider = mapItemGraphicProvider;
            _sfxPlayer = sfxPlayer;
        }

        public TradeDialog Create()
        {
            return new TradeDialog(_nativeGraphicsManager,
                                   _tradeActions,
                                   _localizedStringFinder,
                                   _dialogButtonService,
                                   _messageBoxFactory,
                                   _statusLabelSetter,
                                   _inventorySpaceValidator,
                                   _tradeProvider,
                                   _characterProvider,
                                   _eifFileProvider,
                                   _mapItemGraphicProvider,
                                   _sfxPlayer);
        }
    }

    public interface ITradeDialogFactory
    {
        TradeDialog Create();
    }
}