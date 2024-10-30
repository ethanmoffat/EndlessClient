using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol;

namespace EOLib.Domain.Extensions
{
    public static class MapCoordinateExtensions
    {
        public static MapCoordinate ToCoordinate(this Coords coords) => new MapCoordinate(coords.X, coords.Y);
    }
}
