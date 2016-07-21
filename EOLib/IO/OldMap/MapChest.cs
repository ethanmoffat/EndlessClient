// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Map;

namespace EOLib.IO.OldMap
{
    public class MapChest
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public ChestKey Key { get; set; }
        public byte Slot { get; set; }
        public short ItemID { get; set; }
        public short RespawnTime { get; set; }
        public int Amount { get; set; }
    }
}