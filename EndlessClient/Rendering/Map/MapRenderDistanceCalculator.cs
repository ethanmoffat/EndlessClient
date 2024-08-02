using System;
using AutomaticTypeMapper;
using EOLib.IO.Map;

namespace EndlessClient.Rendering.Map
{
    [AutoMappedType]
    public class MapRenderDistanceCalculator : IMapRenderDistanceCalculator
    {
        private const int DEFAULT_BOUNDS_DISTANCE = 22;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;

        public MapRenderDistanceCalculator(IClientWindowSizeProvider clientWindowSizeProvider)
        {
            _clientWindowSizeProvider = clientWindowSizeProvider;
        }

        public MapRenderBounds CalculateRenderBounds(EOLib.Domain.Character.Character character, IMapFile currentMap)
        {
            var boundsDistanceX = (int)Math.Ceiling((_clientWindowSizeProvider.Resizable ? _clientWindowSizeProvider.Width / 640.0 : 1) * DEFAULT_BOUNDS_DISTANCE);
            var boundsDistanceY = (int)Math.Ceiling((_clientWindowSizeProvider.Resizable ? _clientWindowSizeProvider.Width / 320.0 : 1) * DEFAULT_BOUNDS_DISTANCE);

            var firstRow = Math.Max(character.RenderProperties.MapY - boundsDistanceY, 0);
            var lastRow = Math.Min(character.RenderProperties.MapY + boundsDistanceY, currentMap.Properties.Height);
            var firstCol = Math.Max(character.RenderProperties.MapX - boundsDistanceX, 0);
            var lastCol = Math.Min(character.RenderProperties.MapX + boundsDistanceX, currentMap.Properties.Width);

            return new MapRenderBounds(firstRow, lastRow, firstCol, lastCol);
        }
    }
}