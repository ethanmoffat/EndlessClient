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

        public IReadOnlyList<IMapEntityRenderer> BaseRenderers { get; }

        public IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; }

        public MapEntityRendererProvider(INativeGraphicsManager nativeGraphicsManager,
                                         ICurrentMapProvider currentMapProvider,
                                         ICharacterProvider characterProvider,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         IMapItemGraphicProvider mapItemGraphicProvider,
                                         IRenderOffsetCalculator renderOffsetCalculator,
                                         IClientWindowSizeProvider clientWindowSizeProvider,
                                         IConfigurationProvider configurationProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         INPCRendererProvider npcRendererProvider,
                                         ICharacterStateCache characterStateCache)
        {
            GroundRenderer =
                new GroundLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        renderOffsetCalculator,
                                        clientWindowSizeProvider);

            BaseRenderers = new List<IMapEntityRenderer>
            {
                new AnimatedGroundLayerRenderer(nativeGraphicsManager,
                                                currentMapProvider,
                                                characterProvider,
                                                renderOffsetCalculator,
                                                clientWindowSizeProvider),
                new MapItemLayerRenderer(characterProvider,
                                         renderOffsetCalculator,
                                         clientWindowSizeProvider,
                                         currentMapStateProvider,
                                         mapItemGraphicProvider)
            };

            MapEntityRenderers = new List<IMapEntityRenderer>
            {
                new ShadowLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        renderOffsetCalculator,
                                        configurationProvider,
                                        clientWindowSizeProvider),
                new OverlayLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         renderOffsetCalculator,
                                         clientWindowSizeProvider),
                new MapObjectLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           renderOffsetCalculator,
                                           clientWindowSizeProvider,
                                           currentMapStateProvider),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                renderOffsetCalculator,
                                                clientWindowSizeProvider,
                                                transparent: false),
                new DownWallLayerRenderer(nativeGraphicsManager,
                                          currentMapProvider,
                                          characterProvider,
                                          renderOffsetCalculator,
                                          clientWindowSizeProvider,
                                          currentMapStateProvider),
                new RightWallLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           renderOffsetCalculator,
                                           clientWindowSizeProvider,
                                           currentMapStateProvider),
                new OtherCharacterEntityRenderer(characterProvider,
                                                 characterRendererProvider,
                                                 characterStateCache,
                                                 renderOffsetCalculator,
                                                 clientWindowSizeProvider),
                new NPCEntityRenderer(characterProvider,
                                      renderOffsetCalculator,
                                      npcRendererProvider,
                                      clientWindowSizeProvider),
                new Overlay2LayerRenderer(nativeGraphicsManager,
                                          currentMapProvider,
                                          characterProvider,
                                          renderOffsetCalculator,
                                          clientWindowSizeProvider),
                new RoofLayerRenderer(nativeGraphicsManager,
                                      currentMapProvider,
                                      characterProvider,
                                      renderOffsetCalculator,
                                      clientWindowSizeProvider),
                new OnTopLayerRenderer(nativeGraphicsManager,
                                       currentMapProvider,
                                       characterProvider,
                                       renderOffsetCalculator,
                                       clientWindowSizeProvider),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                renderOffsetCalculator,
                                                clientWindowSizeProvider,
                                                transparent: true)
            };
        }
    }
}