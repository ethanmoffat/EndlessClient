// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Panels;
using EndlessClient.HUD.StatusBars;
using EndlessClient.Input;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.NPC;
using EndlessClient.UIControls;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.Localization;
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
        private readonly IKeyStateRepository _keyStateRepository;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IStatusLabelTextProvider _statusLabelTextProvider;
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IChatModeCalculator _chatModeCalculator;
        private readonly IExperienceTableProvider _experienceTableProvider;

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
                                  IKeyStateRepository keyStateRepository,
                                  IStatusLabelSetter statusLabelSetter,
                                  IStatusLabelTextProvider statusLabelTextProvider,
                                  IContentManagerProvider contentManagerProvider,
                                  IHudControlProvider hudControlProvider,
                                  IChatModeCalculator chatModeCalculator,
                                  IExperienceTableProvider experienceTableProvider)
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
            _keyStateRepository = keyStateRepository;
            _statusLabelSetter = statusLabelSetter;
            _statusLabelTextProvider = statusLabelTextProvider;
            _contentManagerProvider = contentManagerProvider;
            _hudControlProvider = hudControlProvider;
            _chatModeCalculator = chatModeCalculator;
            _experienceTableProvider = experienceTableProvider;
        }

        public void InjectChatController(IChatController chatController)
        {
            _chatController = chatController;
        }

        public IReadOnlyDictionary<HudControlIdentifier, IGameComponent> CreateHud()
        {
            var controls = new Dictionary<HudControlIdentifier, IGameComponent>
            {
                {HudControlIdentifier.CurrentKeyStateTracker, CreateCurrentKeyStateTracker()},

                {HudControlIdentifier.MapRenderer, _mapRendererFactory.CreateMapRenderer()},

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

                {HudControlIdentifier.HPStatusBar, CreateHPStatusBar()},
                {HudControlIdentifier.TPStatusBar, CreateTPStatusBar()},
                {HudControlIdentifier.SPStatusBar, CreateSPStatusBar()},
                {HudControlIdentifier.TNLStatusBar, CreateTNLStatusBar()},

                {HudControlIdentifier.ChatModePictureBox, CreateChatModePictureBox()},
                {HudControlIdentifier.ChatTextBox, CreateChatTextBox()},
                {HudControlIdentifier.ClockLabel, CreateClockLabel()},
                {HudControlIdentifier.StatusLabel, CreateStatusLabel()},

                {HudControlIdentifier.UsageTracker, CreateUsageTracker()},
                {HudControlIdentifier.UserInputHandler, CreateUserInputHandler()},
                {HudControlIdentifier.CharacterAnimator, CreateCharacterAnimator()},
                {HudControlIdentifier.NPCAnimator, CreateNPCAnimator()},
                {HudControlIdentifier.PreviousKeyStateTracker, CreatePreviousKeyStateTracker()}
            };

            return controls;
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
            retButton.OnClick += (o, e) => DoHudStateChangeClick(whichState);
            retButton.OnMouseEnter += (o, e) => _statusLabelSetter.SetStatusLabel(
                EOResourceID.STATUS_LABEL_TYPE_BUTTON,
                EOResourceID.STATUS_LABEL_HUD_BUTTON_HOVER_FIRST + buttonIndex);
            return retButton;
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

            //news is visible by default when loading the game
            if (whichState != InGameStates.News)
                retPanel.Visible = false;

            return retPanel;
        }

        private IGameComponent CreateHPStatusBar()
        {
            return new HPStatusBar(_nativeGraphicsManager, (ICharacterProvider)_characterRepository)
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
        }

        private IGameComponent CreateTPStatusBar()
        {
            return new TPStatusBar(_nativeGraphicsManager, (ICharacterProvider)_characterRepository)
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
        }

        private IGameComponent CreateSPStatusBar()
        {
            return new SPStatusBar(_nativeGraphicsManager, (ICharacterProvider)_characterRepository)
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
        }

        private IGameComponent CreateTNLStatusBar()
        {
            return new TNLStatusBar(_nativeGraphicsManager, (ICharacterProvider)_characterRepository, _experienceTableProvider)
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
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
            var chatTextBox = new ChatTextBox(_contentManagerProvider)
            {
                Text = "",
                Selected = true,
                Visible = true,
                DrawOrder = HUD_CONTROL_LAYER
            };
            chatTextBox.OnEnterPressed += async (o, e) => await _chatController.SendChatAndClearTextBox();
            chatTextBox.OnClicked += (o, e) => _chatController.SelectChatTextBox();

            return chatTextBox;
        }

        private TimeLabel CreateClockLabel()
        {
            return new TimeLabel(_clientWindowSizeProvider) { DrawOrder = HUD_CONTROL_LAYER };
        }

        private UsageTrackerComponent CreateUsageTracker()
        {
            return new UsageTrackerComponent(_endlessGameProvider, _characterRepository);
        }

        private StatusBarLabel CreateStatusLabel()
        {
            return new StatusBarLabel(_clientWindowSizeProvider, _statusLabelTextProvider) { DrawOrder = HUD_CONTROL_LAYER };
        }

        private CurrentKeyStateTracker CreateCurrentKeyStateTracker()
        {
            return new CurrentKeyStateTracker(_endlessGameProvider, _keyStateRepository);
        }

        private IUserInputHandler CreateUserInputHandler()
        {
            return _userInputHandlerFactory.CreateUserInputHandler();
        }

        private ICharacterAnimator CreateCharacterAnimator()
        {
            return new CharacterAnimator(_endlessGameProvider, _characterRepository, _currentMapStateRepository);
        }

        private INPCAnimator CreateNPCAnimator()
        {
            return new NPCAnimator(_endlessGameProvider, _currentMapStateRepository);
        }

        private PreviousKeyStateTracker CreatePreviousKeyStateTracker()
        {
            return new PreviousKeyStateTracker(_endlessGameProvider, _keyStateRepository);
        }
    }
}