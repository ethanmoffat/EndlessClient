// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    public interface IMapFile : IReadOnlyMapFile
    {
        IMapFile RemoveNPCSpawn(NPCSpawnMapEntity spawn);

        IMapFile RemoveChestSpawn(ChestSpawnMapEntity spawn);

        IMapFile RemoveTileAt(int x, int y);

        IMapFile RemoveWarp(WarpMapEntity warp);

        IMapFile RemoveWarpAt(int x, int y);
    }
}
