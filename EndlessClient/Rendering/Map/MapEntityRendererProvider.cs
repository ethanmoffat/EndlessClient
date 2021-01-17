using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.MapEntityRenderers;
using EndlessClient.Rendering.NPC;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;

namespace EndlessClient.Rendering.Map
{
    [MappedType(BaseType = typeof(IMapEntityRendererProvider), IsSingleton = true)]
    public class MapEntityRendererProvider : IMapEntityRendererProvider
    {
        public IMapEntityRenderer GroundRenderer { get; }

        public IMapEntityRenderer ItemRenderer { get; }

        public IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; }

        public MapEntityRendererProvider(INativeGraphicsManager nativeGraphicsManager,
                                         ICurrentMapProvider currentMapProvider,
                                         ICharacterProvider characterProvider,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         IMapItemGraphicProvider mapItemGraphicProvider,
                                         IRenderOffsetCalculator renderOffsetCalculator,
                                         IConfigurationProvider configurationProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         INPCRendererProvider npcRendererProvider,
                                         IChatBubbleProvider chatBubbleProvider,
                                         ICharacterStateCache characterStateCache)
        {
            GroundRenderer =
                new GroundLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        renderOffsetCalculator);
            ItemRenderer =
                new MapItemLayerRenderer(characterProvider,
                                         renderOffsetCalculator,
                                         currentMapStateProvider,
                                         mapItemGraphicProvider);

            MapEntityRenderers = new List<IMapEntityRenderer>
            {
                new OverlayLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         renderOffsetCalculator),
                new ShadowLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        renderOffsetCalculator,
                                        configurationProvider),
                new DownWallLayerRenderer(nativeGraphicsManager,
                                          currentMapProvider,
                                          characterProvider,
                                          renderOffsetCalculator,
                                          currentMapStateProvider),
                new RightWallLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           renderOffsetCalculator,
                                           currentMapStateProvider),
                new MapObjectLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           renderOffsetCalculator),
                new OtherCharacterEntityRenderer(characterProvider,
                                                 characterRendererProvider,
                                                 chatBubbleProvider,
                                                 characterStateCache,
                                                 renderOffsetCalculator),
                new NPCEntityRenderer(characterProvider,
                                      renderOffsetCalculator,
                                      npcRendererProvider,
                                      chatBubbleProvider),
                new Overlay2LayerRenderer(nativeGraphicsManager,
                                      currentMapProvider,
                                      characterProvider,
                                      renderOffsetCalculator),
                new RoofLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         renderOffsetCalculator),
                new OnTopLayerRenderer(nativeGraphicsManager,
                                       currentMapProvider,
                                       characterProvider,
                                       renderOffsetCalculator),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                chatBubbleProvider,
                                                renderOffsetCalculator)
            };
        }

        public void Dispose()
        {
            GroundRenderer.Dispose();
            ItemRenderer.Dispose();

            foreach (var renderer in MapEntityRenderers)
                renderer.Dispose();
        }
    }
}