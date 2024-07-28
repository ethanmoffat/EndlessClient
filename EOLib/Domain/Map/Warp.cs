using Amadevus.RecordGenerator;
using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    [Record(Features.ObjectEquals)]
    public sealed partial class Warp
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
}