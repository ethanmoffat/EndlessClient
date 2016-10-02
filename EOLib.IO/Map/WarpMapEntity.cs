// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    public class WarpMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 8;

        public int X { get; set; }

        public int Y { get; set; }

        public short DestinationMapID { get; set; }

        public byte DestinationMapX { get; set; }

        public byte DestinationMapY { get; set; }

        public byte LevelRequirement { get; set; }

        public DoorSpec DoorType { get; set; }
    }
}
