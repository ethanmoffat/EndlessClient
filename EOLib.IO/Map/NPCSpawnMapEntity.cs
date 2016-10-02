// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    public class NPCSpawnMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 8;

        public int X { get; private set; }

        public int Y { get; private set; }

        public short ID { get; private set; }

        public byte SpawnType { get; private set; }

        public short RespawnTime { get; private set; }

        public byte Amount { get; private set; }

        public NPCSpawnMapEntity()
            : this(-1, -1, -1, 0, -1, 0)
        { }

        private NPCSpawnMapEntity(int x,
                                  int y,
                                  short id,
                                  byte spawnType,
                                  short respawnTime,
                                  byte amount)
        {
            X = x;
            Y = y;
            ID = id;
            SpawnType = spawnType;
            RespawnTime = respawnTime;
            Amount = amount;
        }

        public NPCSpawnMapEntity WithX(int x)
        {
            var newEntity = MakeCopy(this);
            newEntity.X = x;
            return newEntity;
        }

        public NPCSpawnMapEntity WithY(int y)
        {
            var newEntity = MakeCopy(this);
            newEntity.Y = y;
            return newEntity;
        }

        public NPCSpawnMapEntity WithID(short id)
        {
            var newEntity = MakeCopy(this);
            newEntity.ID = id;
            return newEntity;
        }

        public NPCSpawnMapEntity WithSpawnType(byte spawnType)
        {
            var newEntity = MakeCopy(this);
            newEntity.SpawnType = spawnType;
            return newEntity;
        }

        public NPCSpawnMapEntity WithRespawnTime(short respawnTime)
        {
            var newEntity = MakeCopy(this);
            newEntity.RespawnTime = respawnTime;
            return newEntity;
        }

        public NPCSpawnMapEntity WithAmount(byte amount)
        {
            var newEntity = MakeCopy(this);
            newEntity.Amount = amount;
            return newEntity;
        }

        private static NPCSpawnMapEntity MakeCopy(NPCSpawnMapEntity src)
        {
            return new NPCSpawnMapEntity(
                src.X, src.Y, src.ID, src.SpawnType, src.RespawnTime, src.Amount);
        }
    }
}
