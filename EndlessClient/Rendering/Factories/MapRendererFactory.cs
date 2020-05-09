using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Map;
using EndlessClient.Rendering.MapEntityRenderers;
using EndlessClient.Rendering.NPC;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Map;

namespace EndlessClient.Rendering.Factories
{
    [MappedType(BaseType = typeof(IMapRendererFactory))]
    public class MapRendererFactory : IMapRendererFactory
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IMapEntityRendererProvider _mapEntityRendererProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMapRenderDistanceCalculator _mapRenderDistanceCalculator;
        private readonly ICharacterRendererUpdater _characterRendererUpdater;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IMouseCursorRendererFactory _mouseCursorRendererFactory;
        private readonly INPCRendererUpdater _npcRendererUpdater;
        private readonly IDoorStateUpdater _doorStateUpdater;
        private readonly IChatBubbleUpdater _chatBubbleUpdater;

        public MapRendererFactory(IEndlessGameProvider endlessGameProvider,
            IRenderTargetFactory renderTargetFactory,
            IMapEntityRendererProvider mapEntityRendererProvider,
            ICharacterProvider characterProvider,
            ICurrentMapProvider currentMapProvider,
            IMapRenderDistanceCalculator mapRenderDistanceCalculator,
            ICharacterRendererUpdater characterRendererUpdater,
            INPCRendererUpdater npcRendererUpdater,
            IDoorStateUpdater doorStateUpdater,
            IChatBubbleUpdater chatBubbleUpdater,
            IConfigurationProvider configurationProvider,
            IMouseCursorRendererFactory mouseCursorRendererFactory)
        {
            _endlessGameProvider = endlessGameProvider;
            _renderTargetFactory = renderTargetFactory;
            _mapEntityRendererProvider = mapEntityRendererProvider;
            _characterProvider = characterProvider;
            _currentMapProvider = currentMapProvider;
            _mapRenderDistanceCalculator = mapRenderDistanceCalculator;
            _characterRendererUpdater = characterRendererUpdater;
            _npcRendererUpdater = npcRendererUpdater;
            _doorStateUpdater = doorStateUpdater;
            _chatBubbleUpdater = chatBubbleUpdater;
            _configurationProvider = configurationProvider;
            _mouseCursorRendererFactory = mouseCursorRendererFactory;
        }

        public IMapRenderer CreateMapRenderer()
        {
            var mouseCursorRenderer = _mouseCursorRendererFactory.Create();
            return new MapRenderer(_endlessGameProvider.Game,
                                   _renderTargetFactory,
                                   _mapEntityRendererProvider,
                                   _characterProvider,
                                   _currentMapProvider,
                                   _mapRenderDistanceCalculator,
                                   _characterRendererUpdater,
                                   _npcRendererUpdater,
                                   _doorStateUpdater,
                                   _chatBubbleUpdater,
                                   _configurationProvider,
                                   mouseCursorRenderer);
        }
    }
}