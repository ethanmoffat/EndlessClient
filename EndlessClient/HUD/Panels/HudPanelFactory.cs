// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.ControlSets;
using EndlessClient.Rendering.Chat;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.HUD.Panels
{
    public class HudPanelFactory : IHudPanelFactory
    {
        private const int HUD_CONTROL_LAYER = 130;

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly INewsProvider _newsProvider;
        private readonly IChatProvider _chatProvider;

        public HudPanelFactory(INativeGraphicsManager nativeGraphicsManager,
                               IContentManagerProvider contentManagerProvider,
                               IHudControlProvider hudControlProvider,
                               INewsProvider newsProvider,
                               IChatProvider chatProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _contentManagerProvider = contentManagerProvider;
            _hudControlProvider = hudControlProvider;
            _newsProvider = newsProvider;
            _chatProvider = chatProvider;
        }

        public NewsPanel CreateNewsPanel()
        {
            var chatFont = _contentManagerProvider.Content.Load<SpriteFont>(Constants.FontSize08);

            return new NewsPanel(_nativeGraphicsManager,
                                 new ChatRenderableGenerator(chatFont),
                                 _newsProvider,
                                 chatFont) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public InventoryPanel CreateInventoryPanel()
        {
            return new InventoryPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public ActiveSpellsPanel CreateActiveSpellsPanel()
        {
            return new ActiveSpellsPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public PassiveSpellsPanel CreatePassiveSpellsPanel()
        {
            return new PassiveSpellsPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public ChatPanel CreateChatPanel()
        {
            var chatFont = _contentManagerProvider.Content.Load<SpriteFont>(Constants.FontSize08);

            return new ChatPanel(_nativeGraphicsManager,
                                 new ChatRenderableGenerator(chatFont),
                                 _chatProvider,
                                 _hudControlProvider,
                                 chatFont) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public StatsPanel CreateStatsPanel()
        {
            return new StatsPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public OnlineListPanel CreateOnlineListPanel()
        {
            return new OnlineListPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public PartyPanel CreatePartyPanel()
        {
            return new PartyPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public SettingsPanel CreateSettingsPanel()
        {
            return new SettingsPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public HelpPanel CreateHelpPanel()
        {
            return new HelpPanel(_nativeGraphicsManager) { DrawOrder = HUD_CONTROL_LAYER };
        }
    }
}