// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.MapEntityRenderers;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;

namespace EndlessClient.Rendering.Map
{
    public class MapEntityRendererProvider : IMapEntityRendererProvider
    {
        public IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; private set; }

        public MapEntityRendererProvider(INativeGraphicsManager nativeGraphicsManager,
                                         ICurrentMapProvider currentMapProvider,
                                         ICharacterProvider characterProvider,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         IMapItemGraphicProvider mapItemGraphicProvider,
                                         ICharacterRenderOffsetCalculator characterRenderOffsetCalculator,
                                         IConfigurationProvider configurationProvider)
        {
            MapEntityRenderers = new List<IMapEntityRenderer>
            {
                new GroundLayerRenderer(nativeGraphicsManager,
                                        currentMapProvider,
                                        characterProvider,
                                        characterRenderOffsetCalculator),
                new MapItemLayerRenderer(characterProvider,
                                         characterRenderOffsetCalculator,
                                         currentMapStateProvider,
                                         mapItemGraphicProvider),
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
            };
        }
    }
}