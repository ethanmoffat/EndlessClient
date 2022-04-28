using AutomaticTypeMapper;
using EOLib.IO.Map;
using System;

namespace EndlessClient.Rendering.Map
{
    [AutoMappedType]
    public class MapRenderDistanceCalculator : IMapRenderDistanceCalculator
    {
        private const int DEFAULT_BOUNDS_DISTANCE = 22;

        public MapRenderBounds CalculateRenderBounds(EOLib.Domain.Character.Character character, IMapFile currentMap)
        {
            var firstRow = Math.Max(character.RenderProperties.MapY - DEFAULT_BOUNDS_DISTANCE, 0);
            var lastRow = Math.Min(character.RenderProperties.MapY + DEFAULT_BOUNDS_DISTANCE, currentMap.Properties.Height);
            var firstCol = Math.Max(character.RenderProperties.MapX - DEFAULT_BOUNDS_DISTANCE, 0);
            var lastCol = Math.Min(character.RenderProperties.MapX + DEFAULT_BOUNDS_DISTANCE, currentMap.Properties.Width);

            return new MapRenderBounds(firstRow, lastRow, firstCol, lastCol);
        }
    }
}