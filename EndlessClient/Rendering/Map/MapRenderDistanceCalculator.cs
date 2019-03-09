// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO.Map;

namespace EndlessClient.Rendering.Map
{
    [MappedType(BaseType = typeof(IMapRenderDistanceCalculator))]
    public class MapRenderDistanceCalculator : IMapRenderDistanceCalculator
    {
        private const int DEFAULT_BOUNDS_DISTANCE = 22;

        public MapRenderBounds CalculateRenderBounds(ICharacter character, IMapFile currentMap)
        {
            var firstRow = Math.Max(character.RenderProperties.MapY - DEFAULT_BOUNDS_DISTANCE, 0);
            var lastRow = Math.Min(character.RenderProperties.MapY + DEFAULT_BOUNDS_DISTANCE, currentMap.Properties.Height);
            var firstCol = Math.Max(character.RenderProperties.MapX - DEFAULT_BOUNDS_DISTANCE, 0);
            var lastCol = Math.Min(character.RenderProperties.MapX + DEFAULT_BOUNDS_DISTANCE, currentMap.Properties.Width);

            return new MapRenderBounds(firstRow, lastRow, firstCol, lastCol);
        }
    }
}