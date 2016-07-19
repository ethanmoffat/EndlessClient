// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Panels;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.Controls
{
    public class HudControlsFactory : IHudControlsFactory
    {
        private const int HUD_BASE_LAYER = 100;
        private const int HUD_CONTROL_LAYER = 130;

        private readonly IHudButtonController _hudButtonController;
        private readonly IHudPanelFactory _hudPanelFactory;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRepository _characterRepository;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IStatusLabelTextProvider _statusLabelTextProvider;

        public HudControlsFactory(IHudButtonController hudButtonController,
                                  IHudPanelFactory hudPanelFactory,
                                  INativeGraphicsManager nativeGraphicsManager,
                                  IGraphicsDeviceProvider graphicsDeviceProvider,
                                  IClientWindowSizeProvider clientWindowSizeProvider,
                                  IEndlessGameProvider endlessGameProvider,
                                  ICharacterRepository characterRepository,
                                  IStatusLabelSetter statusLabelSetter,
                                  IStatusLabelTextProvider statusLabelTextProvider)
        {
            _hudButtonController = hudButtonController;
            _hudPanelFactory = hudPanelFactory;
            _nativeGraphicsManager = nativeGraphicsManager;
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _endlessGameProvider = endlessGameProvider;
            _characterRepository = characterRepository;
            _statusLabelSetter = statusLabelSetter;
            _statusLabelTextProvider = statusLabelTextProvider;
        }

        public IReadOnlyDictionary<HudControlIdentifier, IGameComponent> CreateHud()
        {
            var controls = new Dictionary<HudControlIdentifier, IGameComponent>
            {
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
                
                {HudControlIdentifier.ClockLabel, CreateClockLabel()},
                {HudControlIdentifier.UsageTracker, CreateUsageTracker()},
                {HudControlIdentifier.StatusLabel, CreateStatusLabel()}
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

        private DisposableButton CreateStateChangeButton(InGameStates whichState)
        {
            if (whichState == InGameStates.News)
                throw new ArgumentOutOfRangeException("whichState", "News state does not have a button associated with it");
            var buttonIndex = (int) whichState;

            var mainButtonTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 25);
            var widthDelta = mainButtonTexture.Width/2;
            var heightDelta = mainButtonTexture.Height/11;

            var xPosition = buttonIndex < 6 ? 62 : 590;
            var yPosition = (buttonIndex < 6 ? 330 : 350) + (buttonIndex < 6 ? buttonIndex : buttonIndex - 6)*20;

            var outSprite = new SpriteSheet(mainButtonTexture, new Rectangle(0, heightDelta * buttonIndex, widthDelta, heightDelta));
            var overSprite = new SpriteSheet(mainButtonTexture, new Rectangle(widthDelta, heightDelta * buttonIndex, widthDelta, heightDelta));

            var retButton = new DisposableButton(
                new Vector2(xPosition, yPosition),
                outSprite.GetSourceTexture(),
                overSprite.GetSourceTexture())
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
            retButton.OnClick += (o, e) => DoHudStateChangeClick(whichState);
            retButton.OnMouseEnter += (o, e) => _statusLabelSetter.SetStatusLabel(
                DATCONST2.STATUS_LABEL_TYPE_BUTTON,
                DATCONST2.STATUS_LABEL_HUD_BUTTON_HOVER_FIRST + buttonIndex);
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
                    _statusLabelSetter.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_CHAT_PANEL_NOW_VIEWED);
                    break;
                case InGameStates.Stats:
                    _hudButtonController.ClickStats();
                    _statusLabelSetter.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_STATS_PANEL_NOW_VIEWED);
                    break;
                case InGameStates.OnlineList:
                    _hudButtonController.ClickOnlineList();
                    _statusLabelSetter.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_ONLINE_PLAYERS_NOW_VIEWED);
                    break;
                case InGameStates.Party: _hudButtonController.ClickParty(); break;
                case InGameStates.Macro: break;
                case InGameStates.Settings: _hudButtonController.ClickSettings(); break;
                case InGameStates.Help:
                    _hudButtonController.ClickHelp();
                    _statusLabelSetter.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_HUD_BUTTON_HOVER_LAST);
                    break;
                default: throw new ArgumentOutOfRangeException("whichState", whichState, null);
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
                default: throw new ArgumentOutOfRangeException("whichState", whichState, "Panel specification is out of range.");
            }

            //news is visible by default when loading the game
            if (whichState != InGameStates.News)
                retPanel.Visible = false;

            return retPanel;
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
    }
}