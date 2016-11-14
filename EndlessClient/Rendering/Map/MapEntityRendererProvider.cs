// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.MapEntityRenderers;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;

namespace EndlessClient.Rendering.Map
{
    public class MapEntityRendererProvider : IMapEntityRendererProvider
    {
        public IReadOnlyList<IMapEntityRenderer> MapBaseRenderers { get; private set; }

        public IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; private set; }

        public MapEntityRendererProvider(INativeGraphicsManager nativeGraphicsManager,
                                         ICurrentMapProvider currentMapProvider,
                                         ICharacterProvider characterProvider,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         IMapItemGraphicProvider mapItemGraphicProvider,
                                         IRenderOffsetCalculator renderOffsetCalculator,
                                         IConfigurationProvider configurationProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         ICharacterStateCache characterStateCache)
        {
            MapBaseRenderers = new List<IMapEntityRenderer>
            {
                new GroundLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        renderOffsetCalculator),
                new MapItemLayerRenderer(characterProvider,
                                         renderOffsetCalculator,
                                         currentMapStateProvider,
                                         mapItemGraphicProvider)
            };

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
                new WallLayerRenderer(nativeGraphicsManager,
                                      currentMapProvider,
                                      characterProvider,
                                      renderOffsetCalculator),
                new MapObjectLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           renderOffsetCalculator),
                new OtherCharacterEntityRenderer(characterProvider,
                                                 characterRendererProvider,
                                                 characterStateCache,
                                                 renderOffsetCalculator),
                new NPCEntityRenderer(characterProvider,
                                      currentMapStateProvider,
                                      renderOffsetCalculator),
                new RoofLayerRenderer(nativeGraphicsManager,
                                      currentMapProvider,
                                      characterProvider,
                                      renderOffsetCalculator),
                new UnknownLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         renderOffsetCalculator),
                new OnTopLayerRenderer(nativeGraphicsManager,
                                       currentMapProvider,
                                       characterProvider,
                                       renderOffsetCalculator),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                renderOffsetCalculator)
            };
        }

        public void Dispose()
        {
            foreach (var renderer in MapBaseRenderers)
                renderer.Dispose();
            foreach (var renderer in MapEntityRenderers)
                renderer.Dispose();
        }
    }
}