using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Panels;
using EndlessClient.HUD.Spells;
using EndlessClient.HUD.StatusBars;
using EndlessClient.Input;
using EndlessClient.Network;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Map;
using EndlessClient.Rendering.NPC;
using EndlessClient.UIControls;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Net.Communication;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD.Controls
{
    //todo: this class is doing a lot. Might be a good idea to split it into multiple factories.
    [MappedType(BaseType = typeof(IHudControlsFactory), IsSingleton = true)]
    public class HudControlsFactory : IHudControlsFactory
    {
        private const int HUD_BASE_LAYER = 100;
        private const int HUD_CONTROL_LAYER = 130;

        private readonly IHudButtonController _hudButtonController;
        private readonly IHudPanelFactory _hudPanelFactory;
        private readonly IMapRendererFactory _mapRendererFactory;
        private readonly IUserInputHandlerFactory _userInputHandlerFactory;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IUserInputRepository _userInputRepository;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IStatusLabelTextProvider _statusLabelTextProvider;
        private readonly IContentProvider _contentProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IChatModeCalculator _chatModeCalculator;
        private readonly IExperienceTableProvider _experienceTableProvider;
        private readonly IPathFinder _pathFinder;
        private readonly ICharacterActions _characterActions;
        private readonly IWalkValidationActions _walkValidationActions;
        private readonly IPacketSendService _packetSendService;
        private readonly IUserInputTimeProvider _userInputTimeProvider;
        private readonly ISpellSlotDataRepository _spellSlotDataRepository;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IMiniMapRendererFactory _miniMapRendererFactory;
        private readonly INewsProvider _newsProvider;
        private IChatController _chatController;

        public HudControlsFactory(IHudButtonController hudButtonController,
                                  IHudPanelFactory hudPanelFactory,
                                  IMapRendererFactory mapRendererFactory,
                                  IUserInputHandlerFactory userInputHandlerFactory,
                                  INativeGraphicsManager nativeGraphicsManager,
                                  IGraphicsDeviceProvider graphicsDeviceProvider,
                                  IClientWindowSizeProvider clientWindowSizeProvider,
                                  IEndlessGameProvider endlessGameProvider,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  IUserInputRepository userInputRepository,
                                  IStatusLabelSetter statusLabelSetter,
                                  IStatusLabelTextProvider statusLabelTextProvider,
                                  IContentProvider contentProvider,
                                  IHudControlProvider hudControlProvider,
                                  ICurrentMapProvider currentMapProvider,
                                  IChatModeCalculator chatModeCalculator,
                                  IExperienceTableProvider experienceTableProvider,
                                  IPathFinder pathFinder,
                                  ICharacterActions characterActions,
                                  IWalkValidationActions walkValidationActions,
                                  IPacketSendService packetSendService,
                                  IUserInputTimeProvider userInputTimeProvider,
                                  ISpellSlotDataRepository spellSlotDataRepository,
                                  ISfxPlayer sfxPlayer,
                                  IMiniMapRendererFactory miniMapRendererFactory,
                                  INewsProvider newsProvider)
        {
            _hudButtonController = hudButtonController;
            _hudPanelFactory = hudPanelFactory;
            _mapRendererFactory = mapRendererFactory;
            _userInputHandlerFactory = userInputHandlerFactory;
            _nativeGraphicsManager = nativeGraphicsManager;
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _endlessGameProvider = endlessGameProvider;
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _userInputRepository = userInputRepository;
            _statusLabelSetter = statusLabelSetter;
            _statusLabelTextProvider = statusLabelTextProvider;
            _contentProvider = contentProvider;
            _hudControlProvider = hudControlProvider;
            _currentMapProvider = currentMapProvider;
            _chatModeCalculator = chatModeCalculator;
            _experienceTableProvider = experienceTableProvider;
            _pathFinder = pathFinder;
            _characterActions = characterActions;
            _walkValidationActions = walkValidationActions;
            _packetSendService = packetSendService;
            _userInputTimeProvider = userInputTimeProvider;
            _spellSlotDataRepository = spellSlotDataRepository;
            _sfxPlayer = sfxPlayer;
            _miniMapRendererFactory = miniMapRendererFactory;
            _newsProvider = newsProvider;
        }

        public void InjectChatController(IChatController chatController)
        {
            _chatController = chatController;
        }

        public IReadOnlyDictionary<HudControlIdentifier, IGameComponent> CreateHud()
        {
            var characterAnimator = CreateCharacterAnimator();

            var controls = new Dictionary<HudControlIdentifier, IGameComponent>
            {
                {HudControlIdentifier.CurrentUserInputTracker, CreateCurrentUserInputTracker()},

                {HudControlIdentifier.MapRenderer, _mapRendererFactory.CreateMapRenderer()},
                {HudControlIdentifier.StatusIcons, CreatePlayerStatusIconRenderer()},
                {HudControlIdentifier.MiniMapRenderer, _miniMapRendererFactory.Create()},

                {HudControlIdentifier.HudBackground, CreateHudBackground()},

                {HudControlIdentifier.InventoryButton, CreateStateChangeButton(InGameStates.Inventory)},
                {HudControlIdentifier.ViewMapButton, CreateStateChangeButton(InGameStates.ViewMapToggle)},
                {HudControlIdentifier.ActiveSpellsButton, CreateStateChangeButton(InGameStates.ActiveSpells)},
                {HudControlIdentifier.PassiveSpellsButton, CreateStateChangeButton(InGameStates.PassiveSpells)},
                {HudControlIdentifier.ChatButton, CreateStateChangeButton(InGameStates.Chat)},
                {HudControlIdentifier.StatsButton, CreateStateChangeButton(InGameStates.Stats)},
                {HudControlIdentifier.OnlineListButton, CreateStateChangeButton(InGameStates.OnlineList)},
                {HudControlIdentifier.PartyButton, CreateStateChangeButton(InGameStates.Party)},
                {HudControlIdentifier.MacroButton, CreateStateChangeButton(InGameStates.Macro)},
                {HudControlIdentifier.SettingsButton, CreateStateChangeButton(InGameStates.Settings)},
                {HudControlIdentifier.HelpButton, CreateStateChangeButton(InGameStates.Help)},

                {HudControlIdentifier.FriendList, CreateFriendListButton()},
                {HudControlIdentifier.IgnoreList, CreateIgnoreListButton()},

                {HudControlIdentifier.NewsPanel, CreateStatePanel(InGameStates.News)},
                {HudControlIdentifier.InventoryPanel, CreateStatePanel(InGameStates.Inventory)},
                {HudControlIdentifier.ActiveSpellsPanel, CreateStatePanel(InGameStates.ActiveSpells)},
                {HudControlIdentifier.PassiveSpellsPanel, CreateStatePanel(InGameStates.PassiveSpells)},
                {HudControlIdentifier.ChatPanel, CreateStatePanel(InGameStates.Chat)},
                {HudControlIdentifier.StatsPanel, CreateStatePanel(InGameStates.Stats)},
                {HudControlIdentifier.OnlineListPanel, CreateStatePanel(InGameStates.OnlineList)},
                {HudControlIdentifier.PartyPanel, CreateStatePanel(InGameStates.Party)},
                //macro panel
                {HudControlIdentifier.SettingsPanel, CreateStatePanel(InGameStates.Settings)},
                {HudControlIdentifier.HelpPanel, CreateStatePanel(InGameStates.Help)},

                {HudControlIdentifier.SessionExpButton, CreateSessionExpButton()},
                {HudControlIdentifier.QuestsButton, CreateQuestButton()},

                {HudControlIdentifier.HPStatusBar, CreateHPStatusBar()},
                {HudControlIdentifier.TPStatusBar, CreateTPStatusBar()},
                {HudControlIdentifier.SPStatusBar, CreateSPStatusBar()},
                {HudControlIdentifier.TNLStatusBar, CreateTNLStatusBar()},

                {HudControlIdentifier.ChatModePictureBox, CreateChatModePictureBox()},
                {HudControlIdentifier.ChatTextBox, CreateChatTextBox()},
                {HudControlIdentifier.ClockLabel, CreateClockLabel()},
                {HudControlIdentifier.StatusLabel, CreateStatusLabel()},

                {HudControlIdentifier.PeriodicStatUpdater, CreatePeriodicStatUpdater()},
                {HudControlIdentifier.UserInputHandler, CreateUserInputHandler()},
                {HudControlIdentifier.CharacterAnimator, characterAnimator},
                {HudControlIdentifier.NPCAnimator, CreateNPCAnimator()},
                {HudControlIdentifier.UnknownEntitiesRequester, CreateUnknownEntitiesRequester()},
                {HudControlIdentifier.PeriodicEmoteHandler, CreatePeriodicEmoteHandler(characterAnimator)},

                {HudControlIdentifier.PreviousUserInputTracker, CreatePreviousUserInputTracker()}
            };

            return controls;
        }

        private PlayerStatusIconRenderer CreatePlayerStatusIconRenderer()
        {
            return new PlayerStatusIconRenderer(_nativeGraphicsManager, (ICharacterProvider)_characterRepository, (ISpellSlotDataProvider)_spellSlotDataRepository, _currentMapProvider);
        }

        private HudBackgroundFrame CreateHudBackground()
        {
            return new HudBackgroundFrame(_nativeGraphicsManager, _graphicsDeviceProvider)
            {
                DrawOrder = HUD_BASE_LAYER
            };
        }

        private IXNAButton CreateStateChangeButton(InGameStates whichState)
        {
            if (whichState == InGameStates.News)
                throw new ArgumentOutOfRangeException(nameof(whichState), "News state does not have a button associated with it");
            var buttonIndex = (int) whichState;

            var mainButtonTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 25);
            var widthDelta = mainButtonTexture.Width/2;
            var heightDelta = mainButtonTexture.Height/11;

            var xPosition = buttonIndex < 6 ? 62 : 590;
            var yPosition = (buttonIndex < 6 ? 330 : 350) + (buttonIndex < 6 ? buttonIndex : buttonIndex - 6)*20;

            var retButton = new XNAButton(
                mainButtonTexture,
                new Vector2(xPosition, yPosition),
                new Rectangle(0, heightDelta * buttonIndex, widthDelta, heightDelta),
                new Rectangle(widthDelta, heightDelta * buttonIndex, widthDelta, heightDelta))
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
            retButton.OnClick += (_, _) => DoHudStateChangeClick(whichState);
            retButton.OnMouseEnter += (_, _) => _statusLabelSetter.SetStatusLabel(
                EOResourceID.STATUS_LABEL_TYPE_BUTTON,
                EOResourceID.STATUS_LABEL_HUD_BUTTON_HOVER_FIRST + buttonIndex);
            return retButton;
        }

        private IXNAButton CreateFriendListButton()
        {
            var button = new XNAButton(
                _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27, false),
                new Vector2(592, 312),
                new Rectangle(0, 260, 17, 15),
                new Rectangle(0, 276, 17, 15))
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
            button.OnClick += (_, _) => _hudButtonController.ClickFriendList();
            button.OnClick += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.ButtonClick);
            button.OnMouseOver += (o, e) => _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_BUTTON, EOResourceID.STATUS_LABEL_FRIEND_LIST);

            return button;
        }

        private IXNAButton CreateIgnoreListButton()
        {
            var button = new XNAButton(
                _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27, false),
                new Vector2(609, 312),
                new Rectangle(17, 260, 17, 15),
                new Rectangle(17, 276, 17, 15))
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
            button.OnClick += (_, _) => _hudButtonController.ClickIgnoreList();
            button.OnClick += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.ButtonClick);
            button.OnMouseOver += (o, e) => _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_BUTTON, EOResourceID.STATUS_LABEL_IGNORE_LIST);

            return button;
        }

        private void DoHudStateChangeClick(InGameStates whichState)
        {
            switch (whichState)
            {
                case InGameStates.Inventory: _hudButtonController.ClickInventory(); break;
                case InGameStates.ViewMapToggle: _hudButtonController.ClickViewMapToggle(); break;
                case InGameStates.ActiveSpells: _hudButtonController.ClickActiveSpells(); break;
                case InGameStates.PassiveSpells: _hudButtonController.ClickPassiveSpells(); break;
                case InGameStates.Chat: 
                    _hudButtonController.ClickChat();
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_CHAT_PANEL_NOW_VIEWED);
                    break;
                case InGameStates.Stats:
                    _hudButtonController.ClickStats();
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_STATS_PANEL_NOW_VIEWED);
                    break;
                case InGameStates.OnlineList:
                    _hudButtonController.ClickOnlineList();
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_ONLINE_PLAYERS_NOW_VIEWED);
                    break;
                case InGameStates.Party: _hudButtonController.ClickParty(); break;
                case InGameStates.Macro: break;
                case InGameStates.Settings: _hudButtonController.ClickSettings(); break;
                case InGameStates.Help:
                    _hudButtonController.ClickHelp();
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_HUD_BUTTON_HOVER_LAST);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(whichState), whichState, null);
            }

            _sfxPlayer.PlaySfx(SoundEffectID.ButtonClick);
        }

        private IGameComponent CreateStatePanel(InGameStates whichState)
        {
            IHudPanel retPanel;

            switch (whichState)
            {
                case InGameStates.Inventory: retPanel = _hudPanelFactory.CreateInventoryPanel(); break;
                case InGameStates.ActiveSpells: retPanel = _hudPanelFactory.CreateActiveSpellsPanel(); break;
                case InGameStates.PassiveSpells: retPanel = _hudPanelFactory.CreatePassiveSpellsPanel(); break;
                case InGameStates.Chat: retPanel = _hudPanelFactory.CreateChatPanel(); break;
                case InGameStates.Stats: retPanel = _hudPanelFactory.CreateStatsPanel(); break;
                case InGameStates.OnlineList: retPanel = _hudPanelFactory.CreateOnlineListPanel(); break;
                case InGameStates.Party: retPanel = _hudPanelFactory.CreatePartyPanel(); break;
                case InGameStates.Settings: retPanel = _hudPanelFactory.CreateSettingsPanel(); break;
                case InGameStates.Help: retPanel = _hudPanelFactory.CreateHelpPanel(); break;
                case InGameStates.News: retPanel = _hudPanelFactory.CreateNewsPanel(); break;
                default: throw new ArgumentOutOfRangeException(nameof(whichState), whichState, "Panel specification is out of range.");
            }

            //news is visible by default when loading the game if news text is set
            retPanel.Visible = (_newsProvider.NewsText.Any() && whichState == InGameStates.News) ||
                               (!_newsProvider.NewsText.Any() && whichState == InGameStates.Chat);

            return retPanel;
        }

        private IGameComponent CreateSessionExpButton()
        {
            var btn = new XNAButton(
                _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 58, transparent: true),
                new Vector2(55, 0),
                new Rectangle(331, 30, 22, 14),
                new Rectangle(331, 30, 22, 14))
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
            btn.OnClick += (_, _) => _hudButtonController.ClickSessionExp();
            btn.OnClick += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.HudStatusBarClick);
            return btn;

        }

        private IGameComponent CreateQuestButton()
        {
            var btn = new XNAButton(
                _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 58, transparent: true),
                new Vector2(77, 0),
                new Rectangle(353, 30, 22, 14),
                new Rectangle(353, 30, 22, 14))
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
            btn.OnClick += (_, _) => _hudButtonController.ClickQuestStatus();
            btn.OnClick += (_, _) => _sfxPlayer.PlaySfx(SoundEffectID.HudStatusBarClick);
            return btn;
        }

        private IGameComponent CreateHPStatusBar()
        {
            var statusBar = new HPStatusBar(_nativeGraphicsManager, (ICharacterProvider)_characterRepository, _userInputRepository) { DrawOrder = HUD_CONTROL_LAYER };
            statusBar.StatusBarClicked += () => _sfxPlayer.PlaySfx(SoundEffectID.HudStatusBarClick);
            return statusBar;
        }

        private IGameComponent CreateTPStatusBar()
        {
            var statusBar = new TPStatusBar(_nativeGraphicsManager, (ICharacterProvider)_characterRepository, _userInputRepository) { DrawOrder = HUD_CONTROL_LAYER };
            statusBar.StatusBarClicked += () => _sfxPlayer.PlaySfx(SoundEffectID.HudStatusBarClick);
            return statusBar;
        }

        private IGameComponent CreateSPStatusBar()
        {
            var statusBar = new SPStatusBar(_nativeGraphicsManager, (ICharacterProvider)_characterRepository, _userInputRepository) { DrawOrder = HUD_CONTROL_LAYER };
            statusBar.StatusBarClicked += () => _sfxPlayer.PlaySfx(SoundEffectID.HudStatusBarClick);
            return statusBar;
        }

        private IGameComponent CreateTNLStatusBar()
        {
            var statusBar = new TNLStatusBar(_nativeGraphicsManager, (ICharacterProvider)_characterRepository, _userInputRepository, _experienceTableProvider) { DrawOrder = HUD_CONTROL_LAYER };
            statusBar.StatusBarClicked += () => _sfxPlayer.PlaySfx(SoundEffectID.HudStatusBarClick);
            return statusBar;
        }

        private ChatModePictureBox CreateChatModePictureBox()
        {
            var chatModesTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 31);
            var pictureBox = new ChatModePictureBox(_chatModeCalculator, _hudControlProvider, chatModesTexture)
            {
                DrawArea = new Rectangle(16, 309, chatModesTexture.Width, chatModesTexture.Height / 8),
                SourceRectangle = new Rectangle(0, 0, chatModesTexture.Width, chatModesTexture.Height / 8),
                DrawOrder = HUD_CONTROL_LAYER
            };

            return pictureBox;
        }

        private ChatTextBox CreateChatTextBox()
        {
            var chatTextBox = new ChatTextBox(_contentProvider)
            {
                Text = "",
                Selected = true,
                Visible = true,
                DrawOrder = HUD_CONTROL_LAYER
            };
            chatTextBox.OnEnterPressed += (_, _) => _chatController.SendChatAndClearTextBox();
            chatTextBox.OnClicked += (_, _) => _chatController.SelectChatTextBox();

            return chatTextBox;
        }

        private TimeLabel CreateClockLabel()
        {
            return new TimeLabel(_clientWindowSizeProvider) { DrawOrder = HUD_CONTROL_LAYER };
        }

        private PeriodicStatUpdaterComponent CreatePeriodicStatUpdater()
        {
            return new PeriodicStatUpdaterComponent(_endlessGameProvider, _characterRepository);
        }

        private UnknownEntitiesRequester CreateUnknownEntitiesRequester()
        {
            return new UnknownEntitiesRequester(_endlessGameProvider, _currentMapStateRepository, _packetSendService);
        }

        private StatusBarLabel CreateStatusLabel()
        {
            return new StatusBarLabel(_clientWindowSizeProvider, _statusLabelTextProvider) { DrawOrder = HUD_CONTROL_LAYER };
        }

        private CurrentUserInputTracker CreateCurrentUserInputTracker()
        {
            return new CurrentUserInputTracker(_endlessGameProvider, _userInputRepository);
        }

        private IUserInputHandler CreateUserInputHandler()
        {
            return _userInputHandlerFactory.CreateUserInputHandler();
        }

        private ICharacterAnimator CreateCharacterAnimator()
        {
            return new CharacterAnimator(_endlessGameProvider, _characterRepository, _currentMapStateRepository, _currentMapProvider, _spellSlotDataRepository, _userInputRepository, _characterActions, _walkValidationActions, _pathFinder);
        }

        private INPCAnimator CreateNPCAnimator()
        {
            return new NPCAnimator(_endlessGameProvider, _currentMapStateRepository);
        }

        private IPeriodicEmoteHandler CreatePeriodicEmoteHandler(ICharacterAnimator characterAnimator)
        {
            return new PeriodicEmoteHandler(_endlessGameProvider, _characterActions, _userInputTimeProvider, _characterRepository, characterAnimator);
        }

        private PreviousUserInputTracker CreatePreviousUserInputTracker()
        {
            return new PreviousUserInputTracker(_endlessGameProvider, _userInputRepository);
        }
    }
}