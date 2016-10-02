// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    public class WarpMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 8;

        public int X { get; private set; }

        public int Y { get; private set; }

        public short DestinationMapID { get; private set; }

        public byte DestinationMapX { get; private set; }

        public byte DestinationMapY { get; private set; }

        public byte LevelRequirement { get; private set; }

        public DoorSpec DoorType { get; private set; }

        public WarpMapEntity()
            : this(-1, -1, -1, 0, 0, 0, DoorSpec.NoDoor)
        { }

        private WarpMapEntity(int x,
                              int y,
                              short destinationMapID,
                              byte destinationMapX,
                              byte destinationMapY,
                              byte levelRequirement,
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

        public WarpMapEntity WithDestinationMapID(short destinationMapID)
        {
            var newEntity = MakeCopy(this);
            newEntity.DestinationMapID = destinationMapID;
            return newEntity;
        }

        public WarpMapEntity WithDestinationMapX(byte destinationMapX)
        {
            var newEntity = MakeCopy(this);
            newEntity.DestinationMapX = destinationMapX;
            return newEntity;
        }

        public WarpMapEntity WithDestinationMapY(byte destinationMapY)
        {
            var newEntity = MakeCopy(this);
            newEntity.DestinationMapY = destinationMapY;
            return newEntity;
        }

        public WarpMapEntity WithLevelRequirement(byte levelRequirement)
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
