using EOLib.Domain.Map;
using EOLib.IO.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Extensions
{
    public static class MapExtensions
    {
        public static int GetDistanceToClosestTileSpec(this IMapFile map, TileSpec spec, MapCoordinate source)
        {
            var shortestDistance = int.MaxValue;
            for (int row = 0; row <= map.Properties.Height; row++)
            {
                for (int col = 0; col <= map.Properties.Width; col++)
                {
                    if (map.Tiles[row, col] != spec)
                        continue;

                    var distance = Math.Abs(source.X - col) + Math.Abs(source.Y - row);
                    if (distance < shortestDistance)
                        shortestDistance = distance;
                }
            }

            return shortestDistance;
        }

        public static IEnumerable<MapCoordinate> GetTileSpecs(this IMapFile map, params TileSpec[] specs)
        {
            var contains = specs.Length == 1
                ? new Func<TileSpec, bool>(s => specs[0] == s)
                : new Func<TileSpec, bool>(s => specs.Any(x => x == s));

            for (int row = 0; row <= map.Properties.Height; row++)
            {
                for (int col = 0; col <= map.Properties.Width; col++)
                {
                    if (contains(map.Tiles[row, col]))
                    {
                        yield return new MapCoordinate(col, row);
                    }
                }
            }
        }
    }
}