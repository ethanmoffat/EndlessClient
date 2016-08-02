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
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.Map
{
    public class MapEntityRendererProvider : IMapEntityRendererProvider
    {
        public IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; private set; }

        public MapEntityRendererProvider(INativeGraphicsManager nativeGraphicsManager,
                                         IMapFileProvider mapFileProvider,
                                         ICharacterProvider characterProvider,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         IMapItemGraphicProvider mapItemGraphicProvider,
                                         ICharacterRenderOffsetCalculator characterRenderOffsetCalculator,
                                         IConfigurationProvider configurationProvider)
        {
            MapEntityRenderers = new List<IMapEntityRenderer>
            {
                new GroundLayerRenderer(nativeGraphicsManager,
                                        mapFileProvider,
                                        characterProvider,
                                        characterRenderOffsetCalculator),
                new MapItemLayerRenderer(mapFileProvider,
                                         characterProvider,
                                         characterRenderOffsetCalculator,
                                         currentMapStateProvider,
                                         mapItemGraphicProvider),
                new OverlayLayerRenderer(nativeGraphicsManager,
                                         mapFileProvider,
                                         characterProvider,
                                         characterRenderOffsetCalculator),
                new ShadowLayerRenderer(nativeGraphicsManager,
                                        mapFileProvider,
                                        characterProvider,
                                        characterRenderOffsetCalculator,
                                        configurationProvider),
                new WallLayerRenderer(nativeGraphicsManager,
                                      mapFileProvider,
                                      characterProvider,
                                      characterRenderOffsetCalculator),
            };
        }
    }
}