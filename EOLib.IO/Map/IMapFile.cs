// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    public interface IMapFile : IReadOnlyMapFile
    {
        void RemoveNPCSpawn(NPCSpawnMapEntity spawn);

        void RemoveChestSpawn(ChestSpawnMapEntity spawn);

        void RemoveWarp(WarpMapEntity warp);

        void RemoveWarpAt(int x, int y);
    }
}
