using System.Collections.Generic;

namespace EOLib.IO.Map
{
    public interface IMapFile
    {
        IMapFileProperties Properties { get; }

        IReadOnlyMatrix<TileSpec> Tiles { get; }
        IReadOnlyMatrix<WarpMapEntity> Warps { get; }
        IReadOnlyDictionary<MapLayer, IReadOnlyMatrix<int>> GFX { get; }
        IReadOnlyDictionary<MapLayer, IReadOnlyList<int>> EmptyGFXRows { get; }
        IReadOnlyList<NPCSpawnMapEntity> NPCSpawns { get; }
        IReadOnlyList<UnknownMapEntity> Unknowns { get; }
        IReadOnlyList<ChestSpawnMapEntity> Chests { get; }
        IReadOnlyList<SignMapEntity> Signs { get; }

        IMapFile WithMapID(int id);

        IMapFile WithMapProperties(IMapFileProperties mapFileProperties);

        IMapFile WithTiles(Matrix<TileSpec> tiles);

        IMapFile WithWarps(Matrix<WarpMapEntity> warps);

        IMapFile WithGFX(Dictionary<MapLayer, Matrix<int>> gfx, Dictionary<MapLayer, List<int>> emptyLayers);

        IMapFile WithNPCSpawns(List<NPCSpawnMapEntity> npcSpawns);

        IMapFile WithUnknowns(List<UnknownMapEntity> unknowns);

        IMapFile WithChests(List<ChestSpawnMapEntity> chests);

        IMapFile WithSigns(List<SignMapEntity> signs);

        IMapFile RemoveNPCSpawn(NPCSpawnMapEntity spawn);

        IMapFile RemoveChestSpawn(ChestSpawnMapEntity spawn);

        IMapFile RemoveTileAt(int x, int y);

        IMapFile RemoveWarp(WarpMapEntity warp);

        IMapFile RemoveWarpAt(int x, int y);
    }
}
