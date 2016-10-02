// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.IO.Map
{
    public class MapFile : IMapFile
    {
        public const string MapFileFormatString = "maps/{0,5:D5}.emf";

        public IMapFileProperties Properties { get; private set; }

        public IReadOnlyMatrix<TileSpec> Tiles { get { return _mutableTiles; } }
        public IReadOnlyMatrix<WarpMapEntity> Warps { get { return _mutableWarps; } }
        public IReadOnlyDictionary<MapLayer, IReadOnlyMatrix<int>> GFX
        {
            get { return _mutableGFX.ToDictionary(k => k.Key, v => (IReadOnlyMatrix<int>) v.Value); }
        }
        public IReadOnlyList<NPCSpawnMapEntity> NPCSpawns { get { return _mutableNPCSpawns; } }
        public IReadOnlyList<UnknownMapEntity> Unknowns { get { return _mutableUnknowns; } }
        public IReadOnlyList<ChestSpawnMapEntity> Chests { get { return _mutableChestSpawns; } }
        public IReadOnlyList<SignMapEntity> Signs { get { return _mutableSigns; } }

        private Matrix<TileSpec> _mutableTiles;
        private Matrix<WarpMapEntity> _mutableWarps;
        private Dictionary<MapLayer, Matrix<int>> _mutableGFX;
        private List<NPCSpawnMapEntity> _mutableNPCSpawns;
        private List<UnknownMapEntity> _mutableUnknowns;
        private List<ChestSpawnMapEntity> _mutableChestSpawns;
        private List<SignMapEntity> _mutableSigns;

        public MapFile()
            : this(new MapFileProperties(),
            Matrix<TileSpec>.Empty,
            Matrix<WarpMapEntity>.Empty,
            new Dictionary<MapLayer, Matrix<int>>(),
            new List<NPCSpawnMapEntity>(),
            new List<UnknownMapEntity>(),
            new List<ChestSpawnMapEntity>(),
            new List<SignMapEntity>())
        {
            foreach (var layer in (MapLayer[]) Enum.GetValues(typeof(MapLayer)))
                _mutableGFX.Add(layer, Matrix<int>.Empty);
        }

        private MapFile(IMapFileProperties properties,
            Matrix<TileSpec> tiles,
            Matrix<WarpMapEntity> warps,
            Dictionary<MapLayer, Matrix<int>> gfx,
            List<NPCSpawnMapEntity> npcSpawns,
            List<UnknownMapEntity> unknowns,
            List<ChestSpawnMapEntity> chests,
            List<SignMapEntity> signs)
        {
            Properties = properties;
            _mutableTiles = tiles;
            _mutableWarps = warps;
            _mutableGFX = gfx;
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

        public IMapFile WithTiles(Matrix<TileSpec> tiles)
        {
            var newMap = MakeCopy(this);
            newMap._mutableTiles = tiles;
            return newMap;
        }

        public IMapFile WithWarps(Matrix<WarpMapEntity> warps)
        {
            var newMap = MakeCopy(this);
            newMap._mutableWarps = warps;
            return newMap;
        }

        public IMapFile WithGFX(Dictionary<MapLayer, Matrix<int>> gfx)
        {
            var newMap = MakeCopy(this);
            newMap._mutableGFX = gfx;
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
                source._mutableWarps,
                source._mutableGFX,
                source._mutableNPCSpawns,
                source._mutableUnknowns,
                source._mutableChestSpawns,
                source._mutableSigns);
        }
    }
}
