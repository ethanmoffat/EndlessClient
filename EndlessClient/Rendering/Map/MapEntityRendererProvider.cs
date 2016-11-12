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
                                         ICharacterRenderOffsetCalculator characterRenderOffsetCalculator,
                                         IConfigurationProvider configurationProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         ICharacterStateCache characterStateCache)
        {
            MapBaseRenderers = new List<IMapEntityRenderer>
            {
                new GroundLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        characterRenderOffsetCalculator),
                new MapItemLayerRenderer(characterProvider,
                                         characterRenderOffsetCalculator,
                                         currentMapStateProvider,
                                         mapItemGraphicProvider)
            };

            MapEntityRenderers = new List<IMapEntityRenderer>
            {
                new OverlayLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         characterRenderOffsetCalculator),
                new ShadowLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        characterRenderOffsetCalculator,
                                        configurationProvider),
                new WallLayerRenderer(nativeGraphicsManager,
                                      currentMapProvider,
                                      characterProvider,
                                      characterRenderOffsetCalculator),
                new MapObjectLayerRenderer(nativeGraphicsManager,
                                           currentMapProvider,
                                           characterProvider,
                                           characterRenderOffsetCalculator),
                new OtherCharacterEntityRenderer(characterProvider,
                                                 characterRendererProvider,
                                                 characterStateCache,
                                                 characterRenderOffsetCalculator),
                new NPCEntityRenderer(characterProvider,
                                      currentMapStateProvider,
                                      characterRenderOffsetCalculator),
                new RoofLayerRenderer(nativeGraphicsManager,
                                      currentMapProvider,
                                      characterProvider,
                                      characterRenderOffsetCalculator),
                new UnknownLayerRenderer(nativeGraphicsManager,
                                         currentMapProvider,
                                         characterProvider,
                                         characterRenderOffsetCalculator),
                new OnTopLayerRenderer(nativeGraphicsManager,
                                       currentMapProvider,
                                       characterProvider,
                                       characterRenderOffsetCalculator),
                new MainCharacterEntityRenderer(characterProvider,
                                                characterRendererProvider,
                                                characterRenderOffsetCalculator)
            };
        }
    }
}