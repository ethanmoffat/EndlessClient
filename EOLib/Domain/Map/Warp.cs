using EOLib.IO.Map;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public class Warp : IWarp
    {
        private readonly WarpMapEntity _warpEntity;

        public int X => _warpEntity.X;

        public int Y => _warpEntity.Y;

        public DoorSpec DoorType => _warpEntity.DoorType;

        public int LevelRequirement => _warpEntity.LevelRequirement;

        public Warp(WarpMapEntity warpEntity)
        {
            _warpEntity = warpEntity;
        }

        public override bool Equals(object obj)
        {
            return obj is Warp warp &&
                   X == warp.X &&
                   Y == warp.Y &&
                   DoorType == warp.DoorType &&
                   LevelRequirement == warp.LevelRequirement;
        }

        public override int GetHashCode()
        {
            int hashCode = 514290371;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + DoorType.GetHashCode();
            hashCode = hashCode * -1521134295 + LevelRequirement.GetHashCode();
            return hashCode;
        }
    }

    public interface IWarp
    {
        int X { get; }

        int Y { get; }

        DoorSpec DoorType { get; }

        int LevelRequirement { get; }
    }
}
