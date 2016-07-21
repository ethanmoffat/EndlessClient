// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Map;
using EOLib.IO.Map;

namespace EOLib.IO.OldMap
{
    public class Warp : IMapElement
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public short DestinationMapID { get; set; }
        public byte DestinationMapX { get; set; }
        public byte DestinationMapY { get; set; }
        public byte LevelRequirement { get; set; }
        public DoorSpec DoorType { get; set; }
        public bool IsDoorOpened { get; set; }
        public bool DoorPacketSent { get; set; }
    }
}