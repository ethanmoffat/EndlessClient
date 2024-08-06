using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.IO.Map
{
    public class MapFile : IMapFile
    {
        public const string MapFileFormatString = "maps/{0,5:D5}.emf";

        public IMapFileProperties Properties { get; private set; }

        public IReadOnlyMatrix<TileSpec> Tiles => _mutableTiles;
        public IReadOnlyList<int> EmptyTileRows => _mutableEmptyTileRows;
        public IReadOnlyMatrix<WarpMapEntity> Warps => _mutableWarps;
        public IReadOnlyList<int> EmptyWarpRows => _mutableEmptyWarpRows;
        public IReadOnlyDictionary<MapLayer, IReadOnlyMatrix<int>> GFX => _readOnlyGFX;
        public IReadOnlyDictionary<MapLayer, IReadOnlyList<int>> EmptyGFXRows => _readOnlyEmptyGFXRows;
        public IReadOnlyList<NPCSpawnMapEntity> NPCSpawns => _mutableNPCSpawns;
        public IReadOnlyList<UnknownMapEntity> Unknowns => _mutableUnknowns;
        public IReadOnlyList<ChestSpawnMapEntity> Chests => _mutableChestSpawns;
        public IReadOnlyList<SignMapEntity> Signs => _mutableSigns;

        private Matrix<TileSpec> _mutableTiles;
        private List<int> _mutableEmptyTileRows;
        private Matrix<WarpMapEntity> _mutableWarps;
        private List<int> _mutableEmptyWarpRows;
        private Dictionary<MapLayer, Matrix<int>> _mutableGFX;
        private IReadOnlyDictionary<MapLayer, IReadOnlyMatrix<int>> _readOnlyGFX;
        private Dictionary<MapLayer, List<int>> _mutableEmptyGFXRows;
        private IReadOnlyDictionary<MapLayer, IReadOnlyList<int>> _readOnlyEmptyGFXRows;
        private List<NPCSpawnMapEntity> _mutableNPCSpawns;
        private List<UnknownMapEntity> _mutableUnknowns;
        private List<ChestSpawnMapEntity> _mutableChestSpawns;
        private List<SignMapEntity> _mutableSigns;

        public MapFile()
            : this(new MapFileProperties(),
            Matrix<TileSpec>.Empty,
            new List<int>(),
            Matrix<WarpMapEntity>.Empty,
            new List<int>(),
            new Dictionary<MapLayer, Matrix<int>>(),
            new Dictionary<MapLayer, List<int>>(),
            new List<NPCSpawnMapEntity>(),
            new List<UnknownMapEntity>(),
            new List<ChestSpawnMapEntity>(),
            new List<SignMapEntity>())
        {
            foreach (var layer in (MapLayer[])Enum.GetValues(typeof(MapLayer)))
                _mutableGFX.Add(layer, Matrix<int>.Empty);
            SetReadOnlyGFX();
        }

        private MapFile(IMapFileProperties properties,
            Matrix<TileSpec> tiles,
            List<int> emptyTileSpecRows,
            Matrix<WarpMapEntity> warps,
            List<int> emptyWarpRows,
            Dictionary<MapLayer, Matrix<int>> gfx,
            Dictionary<MapLayer, List<int>> emptyGFXRows,
            List<NPCSpawnMapEntity> npcSpawns,
            List<UnknownMapEntity> unknowns,
            List<ChestSpawnMapEntity> chests,
            List<SignMapEntity> signs)
        {
            Properties = properties;
            _mutableTiles = tiles;
            _mutableEmptyTileRows = emptyTileSpecRows;
            _mutableWarps = warps;
            _mutableEmptyWarpRows = emptyWarpRows;
            _mutableGFX = gfx;
            _mutableEmptyGFXRows = emptyGFXRows;
            SetReadOnlyGFX();
            _mutableNPCSpawns = npcSpawns;
            _mutableUnknowns = unknowns;
            _mutableChestSpawns = chests;
            _mutableSigns = signs;
        }

        public IMapFile WithMapID(int id)
        {
            var newProperties = Properties.WithMapID(id);
            return WithMapProperties(newProperties);
        }

        public IMapFile WithMapProperties(IMapFileProperties mapFileProperties)
        {
            var newMap = MakeCopy(this);
            newMap.Properties = mapFileProperties;
            return newMap;
        }

        public IMapFile WithTiles(Matrix<TileSpec> tiles, List<int> emptyTileRows)
        {
            var newMap = MakeCopy(this);
            newMap._mutableTiles = tiles;
            newMap._mutableEmptyTileRows = emptyTileRows;
            return newMap;
        }

        public IMapFile WithWarps(Matrix<WarpMapEntity> warps, List<int> emptyWarpRows)
        {
            var newMap = MakeCopy(this);
            newMap._mutableWarps = warps;
            newMap._mutableEmptyWarpRows = emptyWarpRows;
            return newMap;
        }

        public IMapFile WithGFX(Dictionary<MapLayer, Matrix<int>> gfx, Dictionary<MapLayer, List<int>> emptyRows)
        {
            var newMap = MakeCopy(this);
            newMap._mutableGFX = gfx;
            newMap._mutableEmptyGFXRows = emptyRows;
            SetReadOnlyGFX();
            return newMap;
        }

        public IMapFile WithNPCSpawns(List<NPCSpawnMapEntity> npcSpawns)
        {
            var newMap = MakeCopy(this);
            newMap._mutableNPCSpawns = npcSpawns;
            return newMap;
        }

        public IMapFile WithUnknowns(List<UnknownMapEntity> unknowns)
        {
            var newMap = MakeCopy(this);
            newMap._mutableUnknowns = unknowns;
            return newMap;
        }

        public IMapFile WithChests(List<ChestSpawnMapEntity> chests)
        {
            var newMap = MakeCopy(this);
            newMap._mutableChestSpawns = chests;
            return newMap;
        }

        public IMapFile WithSigns(List<SignMapEntity> signs)
        {
            var newMap = MakeCopy(this);
            newMap._mutableSigns = signs;
            return newMap;
        }

        public IMapFile RemoveNPCSpawn(NPCSpawnMapEntity spawn)
        {
            var updatedSpawns = new List<NPCSpawnMapEntity>(_mutableNPCSpawns);
            updatedSpawns.Remove(spawn);

            var newMap = MakeCopy(this);
            newMap._mutableNPCSpawns = updatedSpawns;
            return newMap;
        }

        public IMapFile RemoveChestSpawn(ChestSpawnMapEntity spawn)
        {
            var updatedSpawns = new List<ChestSpawnMapEntity>(_mutableChestSpawns);
            updatedSpawns.Remove(spawn);

            var newMap = MakeCopy(this);
            newMap._mutableChestSpawns = updatedSpawns;
            return newMap;
        }

        public IMapFile RemoveTileAt(int x, int y)
        {
            var updatedTiles = new Matrix<TileSpec>(_mutableTiles);
            updatedTiles[y, x] = TileSpec.None;

            var newMap = MakeCopy(this);
            newMap._mutableTiles = _mutableTiles;
            return newMap;
        }

        public IMapFile RemoveWarp(WarpMapEntity warp)
        {
            return RemoveWarpAt(warp.X, warp.Y);
        }

        public IMapFile RemoveWarpAt(int x, int y)
        {
            var updatedWarps = new Matrix<WarpMapEntity>(_mutableWarps);
            updatedWarps[y, x] = null;

            var newMap = MakeCopy(this);
            newMap._mutableWarps = updatedWarps;
            return newMap;
        }

        private static MapFile MakeCopy(MapFile source)
        {
            return new MapFile(
                source.Properties,
                source._mutableTiles,
                source._mutableEmptyTileRows,
                source._mutableWarps,
                source._mutableEmptyWarpRows,
                source._mutableGFX,
                source._mutableEmptyGFXRows,
                source._mutableNPCSpawns,
                source._mutableUnknowns,
                source._mutableChestSpawns,
                source._mutableSigns);
        }

        private void SetReadOnlyGFX()
        {
            _readOnlyGFX = _mutableGFX.ToDictionary(k => k.Key, v => (IReadOnlyMatrix<int>)v.Value);
            _readOnlyEmptyGFXRows = _mutableEmptyGFXRows.ToDictionary(k => k.Key, v => (IReadOnlyList<int>)v.Value);
        }
    }
}
