using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD.Inventory;
using EndlessClient.HUD.Spells;
using EndlessClient.Rendering.Chat;
using EndlessClient.Services;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Item;
using EOLib.Domain.Login;
using EOLib.Domain.Online;
using EOLib.Domain.Party;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.HUD.Panels
{
    [MappedType(BaseType = typeof(IHudPanelFactory))]
    public class HudPanelFactory : IHudPanelFactory
    {
        private const int HUD_CONTROL_LAYER = 130;

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IInventoryController _inventoryController;
        private readonly IChatActions _chatActions;
        private readonly IContentProvider _contentProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly INewsProvider _newsProvider;
        private readonly IChatProvider _chatProvider;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IExperienceTableProvider _experienceTableProvider;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IInventorySlotRepository _inventorySlotRepository;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ITrainingController _trainingController;
        private readonly IFriendIgnoreListService _friendIgnoreListService;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IItemStringService _itemStringService;
        private readonly IItemNameColorService _itemNameColorService;
        private readonly IInventoryService _inventoryService;
        private readonly IActiveDialogProvider _activeDialogProvider;
        private readonly ISpellSlotDataRepository _spellSlotDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IOnlinePlayerProvider _onlinePlayerProvider;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IAudioActions _audioActions;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IPartyActions _partyActions;
        private readonly IPartyDataProvider _partyDataProvider;

        public HudPanelFactory(INativeGraphicsManager nativeGraphicsManager,
                               IInventoryController inventoryController,
                               IChatActions chatActions,
                               IContentProvider contentProvider,
                               IHudControlProvider hudControlProvider,
                               INewsProvider newsProvider,
                               IChatProvider chatProvider,
                               IPlayerInfoProvider playerInfoProvider,
                               ICharacterProvider characterProvider,
                               ICharacterInventoryProvider characterInventoryProvider,
                               IExperienceTableProvider experienceTableProvider,
                               IPubFileProvider pubFileProvider,
                               IInventorySlotRepository inventorySlotRepository,
                               IEOMessageBoxFactory messageBoxFactory,
                               ITrainingController trainingController,
                               IFriendIgnoreListService friendIgnoreListService,
                               IStatusLabelSetter statusLabelSetter,
                               IItemStringService itemStringService,
                               IItemNameColorService itemNameColorService,
                               IInventoryService inventoryService,
                               IActiveDialogProvider activeDialogProvider,
                               ISpellSlotDataRepository spellSlotDataRepository,
                               IConfigurationRepository configurationRepository,
                               IOnlinePlayerProvider onlinePlayerProvider,
                               ILocalizedStringFinder localizedStringFinder,
                               IAudioActions audioActions,
                               ISfxPlayer sfxPlayer,
                               IPartyActions partyActions,
                               IPartyDataProvider partyDataProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _inventoryController = inventoryController;
            _chatActions = chatActions;
            _contentProvider = contentProvider;
            _hudControlProvider = hudControlProvider;
            _newsProvider = newsProvider;
            _chatProvider = chatProvider;
            _playerInfoProvider = playerInfoProvider;
            _characterProvider = characterProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _experienceTableProvider = experienceTableProvider;
            _pubFileProvider = pubFileProvider;
            _inventorySlotRepository = inventorySlotRepository;
            _messageBoxFactory = messageBoxFactory;
            _trainingController = trainingController;
            _friendIgnoreListService = friendIgnoreListService;
            _statusLabelSetter = statusLabelSetter;
            _itemStringService = itemStringService;
            _itemNameColorService = itemNameColorService;
            _inventoryService = inventoryService;
            _activeDialogProvider = activeDialogProvider;
            _spellSlotDataRepository = spellSlotDataRepository;
            _configurationRepository = configurationRepository;
            _onlinePlayerProvider = onlinePlayerProvider;
            _localizedStringFinder = localizedStringFinder;
            _audioActions = audioActions;
            _sfxPlayer = sfxPlayer;
            _partyActions = partyActions;
            _partyDataProvider = partyDataProvider;
        }

        public NewsPanel CreateNewsPanel()
        {
            var chatFont = _contentProvider.Fonts[Constants.FontSize08];

            return new NewsPanel(_nativeGraphicsManager,
                                 new ChatRenderableGenerator(_nativeGraphicsManager, _friendIgnoreListService, chatFont),
                                 _newsProvider,
                                 chatFont) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public InventoryPanel CreateInventoryPanel()
        {
            return new InventoryPanel(_nativeGraphicsManager,
                _inventoryController,
                _statusLabelSetter,
                _itemStringService,
                _itemNameColorService,
                _inventoryService,
                _inventorySlotRepository,
                _playerInfoProvider,
                _characterProvider,
                _characterInventoryProvider,
                _pubFileProvider,
                _hudControlProvider,
                _activeDialogProvider,
                _sfxPlayer,
                _configurationRepository) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public ActiveSpellsPanel CreateActiveSpellsPanel()
        {
            return new ActiveSpellsPanel(_nativeGraphicsManager,
                _trainingController,
                _messageBoxFactory,
                _statusLabelSetter,
                _playerInfoProvider,
                _characterProvider,
                _characterInventoryProvider,
                _pubFileProvider,
                _spellSlotDataRepository,
                _sfxPlayer,
                _configurationRepository) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public PassiveSpellsPanel CreatePassiveSpellsPanel()
        {
            return new PassiveSpellsPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public ChatPanel CreateChatPanel()
        {
            var chatFont = _contentProvider.Fonts[Constants.FontSize08];

            return new ChatPanel(_nativeGraphicsManager,
                                 _chatActions,
                                 new ChatRenderableGenerator(_nativeGraphicsManager, _friendIgnoreListService, chatFont),
                                 _chatProvider,
                                 _hudControlProvider,
                                 chatFont) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public StatsPanel CreateStatsPanel()
        {
            return new StatsPanel(_nativeGraphicsManager,
                                  _characterProvider,
                                  _characterInventoryProvider,
                                  _experienceTableProvider,
                                  _messageBoxFactory,
                                  _trainingController) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public OnlineListPanel CreateOnlineListPanel()
        {
            var chatFont = _contentProvider.Fonts[Constants.FontSize08];
            return new OnlineListPanel(_nativeGraphicsManager, _hudControlProvider, _onlinePlayerProvider, _partyDataProvider, _friendIgnoreListService, _sfxPlayer, chatFont) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public PartyPanel CreatePartyPanel()
        {
            return new PartyPanel(_nativeGraphicsManager, _partyActions, _contentProvider, _partyDataProvider, _characterProvider) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public SettingsPanel CreateSettingsPanel()
        {
            return new SettingsPanel(_nativeGraphicsManager,
                _chatActions,
                _audioActions,
                _statusLabelSetter,
                _localizedStringFinder,
                _messageBoxFactory,
                _configurationRepository,
                _sfxPlayer) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public HelpPanel CreateHelpPanel()
        {
            return new HelpPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }
    }
}