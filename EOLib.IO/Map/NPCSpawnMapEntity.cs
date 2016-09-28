// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    public class NPCSpawnMapEntity : IMapEntity
    {
        public int X { get; set; }

        public int Y { get; set; }

        public short ID { get; set; }

        public byte SpawnType { get; set; }

        public short RespawnTime { get; set; }

        public byte Amount { get; set; }
    }
}
