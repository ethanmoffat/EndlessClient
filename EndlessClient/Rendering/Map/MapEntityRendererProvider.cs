using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.MapEntityRenderers;
using EndlessClient.Rendering.NPC;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using System.Collections.Generic;

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
                                         IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                         IClientWindowSizeProvider clientWindowSizeProvider,
                                         IConfigurationProvider configurationProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         INPCRendererProvider npcRendererProvider)
        {
            GroundRenderer =
                new GroundLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        gridDrawCoordinateCalculator,
                                        clientWindowSizeProvider);

            BaseRenderers = new List<IMapEntityRenderer>
            {
                new AnimatedGroundLayerRenderer(nativeGraphicsManager,
                                                currentMapProvider,
                                                characterProvider,
                                                gridDrawCoordinateCalculator,
                                                clientWindowSizeProvider),
                new MapItemLayerRenderer(characterProvider,
                                         gridDrawCoordinateCalculator,
                                         clientWindowSizeProvider,
                                         currentMapStateProvider,
                                         mapItemGraphicProvider)
            };

            MapEntityRenderers = new List<IMapEntityRenderer>
            {
                new ShadowLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        gridDrawCoordinateCalculator,
                                        clientWindowSizeProvider,
                                        configurationProvider),
                new OverlayLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         gridDrawCoordinateCalculator,
                                         clientWindowSizeProvider),
                new MapObjectLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           gridDrawCoordinateCalculator,
                                           clientWindowSizeProvider,
                                           currentMapStateProvider),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                gridDrawCoordinateCalculator,
                                                clientWindowSizeProvider,
                                                transparent: false),
                new DownWallLayerRenderer(nativeGraphicsManager,
                                          currentMapProvider,
                                          characterProvider,
                                          gridDrawCoordinateCalculator,
                                          clientWindowSizeProvider,
                                          currentMapStateProvider),
                new RightWallLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           gridDrawCoordinateCalculator,
                                           clientWindowSizeProvider,
                                           currentMapStateProvider),
                new OtherCharacterEntityRenderer(characterProvider,
                                                 characterRendererProvider,
                                                 currentMapStateProvider,
                                                 gridDrawCoordinateCalculator,
                                                 clientWindowSizeProvider),
                new NPCEntityRenderer(characterProvider,
                                      gridDrawCoordinateCalculator,
                                      clientWindowSizeProvider,
                                      npcRendererProvider,
                                      currentMapStateProvider),
                new Overlay2LayerRenderer(nativeGraphicsManager,
                                          currentMapProvider,
                                          characterProvider,
                                          gridDrawCoordinateCalculator,
                                          clientWindowSizeProvider),
                new RoofLayerRenderer(nativeGraphicsManager,
                                      currentMapProvider,
                                      characterProvider,
                                      gridDrawCoordinateCalculator,
                                      clientWindowSizeProvider),
                new RoofLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         gridDrawCoordinateCalculator,
                                         clientWindowSizeProvider),
                new OnTopLayerRenderer(nativeGraphicsManager,
                                       currentMapProvider,
                                       characterProvider,
                                       gridDrawCoordinateCalculator,
                                       clientWindowSizeProvider),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                gridDrawCoordinateCalculator,
                                                clientWindowSizeProvider,
                                                transparent: true)
            };
        }
    }
}