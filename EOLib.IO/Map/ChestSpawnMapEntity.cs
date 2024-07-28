namespace EOLib.IO.Map
{
    public class ChestSpawnMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 12;

        public int X { get; private set; }

        public int Y { get; private set; }

        public ChestKey Key { get; private set; }

        public int Slot { get; private set; }

        public int ItemID { get; private set; }

        public int RespawnTime { get; private set; }

        public int Amount { get; private set; }

        public ChestSpawnMapEntity()
            : this(-1, -1, ChestKey.None, -1, -1, -1, -1)
        { }

        private ChestSpawnMapEntity(int x,
            int y,
            ChestKey key,
            int slot,
            int itemID,
            int respawnTime,
            int amount)
        {
            X = x;
            Y = y;
            Key = key;
            Slot = slot;
            ItemID = itemID;
            RespawnTime = respawnTime;
            Amount = amount;
        }

        public ChestSpawnMapEntity WithX(int x)
        {
            var newEntity = MakeCopy(this);
            newEntity.X = x;
            return newEntity;
        }

        public ChestSpawnMapEntity WithY(int y)
        {
            var newEntity = MakeCopy(this);
            newEntity.Y = y;
            return newEntity;
        }

        public ChestSpawnMapEntity WithKey(ChestKey key)
        {
            var newEntity = MakeCopy(this);
            newEntity.Key = key;
            return newEntity;
        }

        public ChestSpawnMapEntity WithSlot(int slot)
        {
            var newEntity = MakeCopy(this);
            newEntity.Slot = slot;
            return newEntity;
        }

        public ChestSpawnMapEntity WithItemID(int itemID)
        {
            var newEntity = MakeCopy(this);
            newEntity.ItemID = itemID;
            return newEntity;
        }

        public ChestSpawnMapEntity WithRespawnTime(int respawnTime)
        {
            var newEntity = MakeCopy(this);
            newEntity.RespawnTime = respawnTime;
            return newEntity;
        }

        public ChestSpawnMapEntity WithAmount(int amount)
        {
            var newEntity = MakeCopy(this);
            newEntity.Amount = amount;
            return newEntity;
        }

        private static ChestSpawnMapEntity MakeCopy(ChestSpawnMapEntity src)
        {
            return new ChestSpawnMapEntity(
                src.X, src.Y, src.Key, src.Slot, src.ItemID, src.RespawnTime, src.Amount);
        }
    }
}