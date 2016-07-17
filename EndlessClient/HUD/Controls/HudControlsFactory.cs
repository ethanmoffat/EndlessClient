// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Panels;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.HUD.Controls
{
    public class HudControlsFactory : IHudControlsFactory
    {
        private const int HUD_BASE_LAYER = 100;
        private const int HUD_CONTROL_LAYER = 130;

        private readonly IHudPanelFactory _hudPanelFactory;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRepository _characterRepository;

        public HudControlsFactory(IHudPanelFactory hudPanelFactory,
                                  INativeGraphicsManager nativeGraphicsManager,
                                  IGraphicsDeviceProvider graphicsDeviceProvider,
                                  IClientWindowSizeProvider clientWindowSizeProvider,
                                  IEndlessGameProvider endlessGameProvider,
                                  ICharacterRepository characterRepository)
        {
            _hudPanelFactory = hudPanelFactory;
            _nativeGraphicsManager = nativeGraphicsManager;
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _endlessGameProvider = endlessGameProvider;
            _characterRepository = characterRepository;
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
                //{HudControlIdentifier.InventoryPanel, CreateStatePanel(InGameStates.Inventory)},
                //{HudControlIdentifier.ActiveSpellsPanel, CreateStatePanel(InGameStates.ActiveSpells)},
                //{HudControlIdentifier.PassiveSpellsPanel, CreateStatePanel(InGameStates.PassiveSpells)},
                //{HudControlIdentifier.ChatPanel, CreateStatePanel(InGameStates.Chat)},
                //{HudControlIdentifier.StatsPanel, CreateStatePanel(InGameStates.Stats)},
                //{HudControlIdentifier.OnlineListPanel, CreateStatePanel(InGameStates.OnlineList)},
                //{HudControlIdentifier.PartyPanel, CreateStatePanel(InGameStates.Party)},
                //macro panel
                //{HudControlIdentifier.SettingsPanel, CreateStatePanel(InGameStates.Settings)},
                //{HudControlIdentifier.HelpPanel, CreateStatePanel(InGameStates.Help)},
                
                {HudControlIdentifier.ClockLabel, CreateClockLabel()},
                {HudControlIdentifier.UsageTracker, CreateUsageTracker()}
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
            //retButton.OnClick += //todo: game state controller, set in-game state?
            //retButton.OnMouseOver +=  //todo: set status label
                                        //DATCONST2.STATUS_LABEL_TYPE_BUTTON,
                                        //DATCONST2.STATUS_LABEL_HUD_BUTTON_HOVER_FIRST + buttonIndex
            return retButton;
        }

        private IGameComponent CreateStatePanel(InGameStates whichState)
        {
            IHudPanel retPanel = null;

            switch (whichState)
            {
                //case InGameStates.Inventory:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 44);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                //case InGameStates.ActiveSpells:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 62);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                //case InGameStates.PassiveSpells:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 62);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                //case InGameStates.Chat:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 28);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                //case InGameStates.Stats:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 34);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                //case InGameStates.OnlineList:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 36);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                //case InGameStates.Party:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 42);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                //case InGameStates.Settings:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 47);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                //case InGameStates.Help:
                //    backgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 63);
                //    retPanel = new XNAPanel(new Rectangle(102, 330, backgroundImage.Width, backgroundImage.Height));
                //    break;
                case InGameStates.News: retPanel = _hudPanelFactory.CreateNewsPanel(); break;
                //default:
                //    throw new ArgumentOutOfRangeException("whichState",
                //        whichState,
                //        "Panel specification is out of range.");
            }

            //retPanel.Visible = false;
            //retPanel.DrawOrder = HUD_CONTROL_LAYER;

            return retPanel;
        }

        private TimeLabel CreateClockLabel()
        {
            return new TimeLabel(_clientWindowSizeProvider)
            {
                DrawOrder = HUD_CONTROL_LAYER
            };
        }

        private UsageTrackerComponent CreateUsageTracker()
        {
            return new UsageTrackerComponent(_endlessGameProvider, _characterRepository);
        }
    }
}