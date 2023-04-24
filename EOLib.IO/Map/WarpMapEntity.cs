namespace EOLib.IO.Map
{
    public class WarpMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 8;

        public int X { get; private set; }

        public int Y { get; private set; }

        public int DestinationMapID { get; private set; }

        public int DestinationMapX { get; private set; }

        public int DestinationMapY { get; private set; }

        public int LevelRequirement { get; private set; }

        public DoorSpec DoorType { get; private set; }

        public WarpMapEntity()
            : this(-1, -1, -1, -1, -1, -1, DoorSpec.NoDoor)
        { }

        private WarpMapEntity(int x,
                              int y,
                              int destinationMapID,
                              int destinationMapX,
                              int destinationMapY,
                              int levelRequirement,
                              DoorSpec doorType)
        {
            X = x;
            Y = y;
            DestinationMapID = destinationMapID;
            DestinationMapX = destinationMapX;
            DestinationMapY = destinationMapY;
            LevelRequirement = levelRequirement;
            DoorType = doorType;
        }

        public WarpMapEntity WithX(int x)
        {
            var newEntity = MakeCopy(this);
            newEntity.X = x;
            return newEntity;
        }

        public WarpMapEntity WithY(int y)
        {
            var newEntity = MakeCopy(this);
            newEntity.Y = y;
            return newEntity;
        }

        public WarpMapEntity WithDestinationMapID(int destinationMapID)
        {
            var newEntity = MakeCopy(this);
            newEntity.DestinationMapID = destinationMapID;
            return newEntity;
        }

        public WarpMapEntity WithDestinationMapX(int destinationMapX)
        {
            var newEntity = MakeCopy(this);
            newEntity.DestinationMapX = destinationMapX;
            return newEntity;
        }

        public WarpMapEntity WithDestinationMapY(int destinationMapY)
        {
            var newEntity = MakeCopy(this);
            newEntity.DestinationMapY = destinationMapY;
            return newEntity;
        }

        public WarpMapEntity WithLevelRequirement(int levelRequirement)
        {
            var newEntity = MakeCopy(this);
            newEntity.LevelRequirement = levelRequirement;
            return newEntity;
        }

        public WarpMapEntity WithDoorType(DoorSpec doorType)
        {
            var newEntity = MakeCopy(this);
            newEntity.DoorType = doorType;
            return newEntity;
        }

        private static WarpMapEntity MakeCopy(WarpMapEntity src)
        {
            return new WarpMapEntity(
                src.X,
                src.Y,
                src.DestinationMapID,
                src.DestinationMapX,
                src.DestinationMapY,
                src.LevelRequirement,
                src.DoorType);
        }
    }
}
