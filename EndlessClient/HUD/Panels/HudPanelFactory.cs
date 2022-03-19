using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Rendering.Chat;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.HUD.Panels
{
    [MappedType(BaseType = typeof(IHudPanelFactory))]
    public class HudPanelFactory : IHudPanelFactory
    {
        private const int HUD_CONTROL_LAYER = 130;

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly INewsProvider _newsProvider;
        private readonly IChatProvider _chatProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IExperienceTableProvider _experienceTableProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ITrainingController _trainingController;

        public HudPanelFactory(INativeGraphicsManager nativeGraphicsManager,
                               IContentManagerProvider contentManagerProvider,
                               IHudControlProvider hudControlProvider,
                               INewsProvider newsProvider,
                               IChatProvider chatProvider,
                               ICharacterProvider characterProvider,
                               ICharacterInventoryProvider characterInventoryProvider,
                               IExperienceTableProvider experienceTableProvider,
                               IEOMessageBoxFactory messageBoxFactory,
                               ITrainingController trainingController)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _contentManagerProvider = contentManagerProvider;
            _hudControlProvider = hudControlProvider;
            _newsProvider = newsProvider;
            _chatProvider = chatProvider;
            _characterProvider = characterProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _experienceTableProvider = experienceTableProvider;
            _messageBoxFactory = messageBoxFactory;
            _trainingController = trainingController;
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
            return new StatsPanel(_nativeGraphicsManager,
                                  _characterProvider,
                                  _characterInventoryProvider,
                                  _experienceTableProvider,
                                  _messageBoxFactory,
                                  _trainingController) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public OnlineListPanel CreateOnlineListPanel()
        {
            var chatFont = _contentManagerProvider.Content.Load<SpriteFont>(Constants.FontSize08);
            return new OnlineListPanel(_nativeGraphicsManager, chatFont) { DrawOrder = HUD_CONTROL_LAYER };
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