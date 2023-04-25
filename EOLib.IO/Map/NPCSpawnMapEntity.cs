namespace EOLib.IO.Map
{
    public class NPCSpawnMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 8;

        public int X { get; private set; }

        public int Y { get; private set; }

        public int ID { get; private set; }

        public int SpawnType { get; private set; }

        public int RespawnTime { get; private set; }

        public int Amount { get; private set; }

        public NPCSpawnMapEntity()
            : this(-1, -1, -1, -1, -1, -1)
        { }

        private NPCSpawnMapEntity(int x,
                                  int y,
                                  int id,
                                  int spawnType,
                                  int respawnTime,
                                  int amount)
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

        public NPCSpawnMapEntity WithID(int id)
        {
            var newEntity = MakeCopy(this);
            newEntity.ID = id;
            return newEntity;
        }

        public NPCSpawnMapEntity WithSpawnType(int spawnType)
        {
            var newEntity = MakeCopy(this);
            newEntity.SpawnType = spawnType;
            return newEntity;
        }

        public NPCSpawnMapEntity WithRespawnTime(int respawnTime)
        {
            var newEntity = MakeCopy(this);
            newEntity.RespawnTime = respawnTime;
            return newEntity;
        }

        public NPCSpawnMapEntity WithAmount(int amount)
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
