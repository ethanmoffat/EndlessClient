// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.IO.Map
{
    public static class MapFileExtensions
    {
        public static IMapFile AsEditableMap(this IReadOnlyMapFile mapFile)
        {
            if (!(mapFile is MapFile))
                throw new ArgumentException("Unexpected type of map file!", "mapFile");

            return (MapFile)mapFile;
        }
    }
}
