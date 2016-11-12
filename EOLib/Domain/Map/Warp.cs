// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    public class Warp : IWarp
    {
        private readonly WarpMapEntity _warpEntity;

        public int X { get { return _warpEntity.X; } }

        public int Y { get { return _warpEntity.Y; } }

        public DoorSpec DoorType { get { return _warpEntity.DoorType; } }

        public int LevelRequirement { get { return _warpEntity.LevelRequirement; } }

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
