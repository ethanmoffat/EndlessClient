using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
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
                                         IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                         IConfigurationProvider configurationProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         INPCRendererProvider npcRendererProvider,
                                         ICharacterStateCache characterStateCache)
        {
            GroundRenderer =
                new GroundLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        gridDrawCoordinateCalculator);

            BaseRenderers = new List<IMapEntityRenderer>
            {
                new AnimatedGroundLayerRenderer(nativeGraphicsManager,
                                                currentMapProvider,
                                                characterProvider,
                                                gridDrawCoordinateCalculator),
                new MapItemLayerRenderer(characterProvider,
                                         gridDrawCoordinateCalculator,
                                         currentMapStateProvider,
                                         mapItemGraphicProvider)
            };

            MapEntityRenderers = new List<IMapEntityRenderer>
            {
                new ShadowLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        gridDrawCoordinateCalculator,
                                        configurationProvider),
                new OverlayLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         gridDrawCoordinateCalculator),
                new MapObjectLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           gridDrawCoordinateCalculator,
                                           currentMapStateProvider),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                gridDrawCoordinateCalculator,
                                                transparent: false),
                new DownWallLayerRenderer(nativeGraphicsManager,
                                          currentMapProvider,
                                          characterProvider,
                                          gridDrawCoordinateCalculator,
                                          currentMapStateProvider),
                new RightWallLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           gridDrawCoordinateCalculator,
                                           currentMapStateProvider),
                new OtherCharacterEntityRenderer(characterProvider,
                                                 characterRendererProvider,
                                                 characterStateCache,
                                                 gridDrawCoordinateCalculator),
                new NPCEntityRenderer(characterProvider,
                                      gridDrawCoordinateCalculator,
                                      npcRendererProvider),
                new Overlay2LayerRenderer(nativeGraphicsManager,
                                      currentMapProvider,
                                      characterProvider,
                                      gridDrawCoordinateCalculator),
                new RoofLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         gridDrawCoordinateCalculator),
                new OnTopLayerRenderer(nativeGraphicsManager,
                                       currentMapProvider,
                                       characterProvider,
                                       gridDrawCoordinateCalculator),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                gridDrawCoordinateCalculator,
                                                transparent: true)
            };
        }
    }
}