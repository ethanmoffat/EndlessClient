using EOLib.IO.Map;

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
    }

    public interface IWarp
    {
        int X { get; }

        int Y { get; }

        DoorSpec DoorType { get; }

        int LevelRequirement { get; }
    }
}
