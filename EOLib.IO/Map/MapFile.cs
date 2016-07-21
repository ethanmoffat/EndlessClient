// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public class MapFile : IMapFile
    {
        public IMapFileProperties Properties { get; private set; }

        public IReadOnlyMatrix<TileSpec> Tiles { get; private set; }
        public IReadOnlyMatrix<WarpMapEntity> Warps { get; private set; }
        public IReadOnlyDictionary<MapLayer, IReadOnlyMatrix<int>> GFX { get; private set; }

        public IReadOnlyList<NPCSpawnMapEntity> NPCSpawns { get; private set; }
        public IReadOnlyList<byte[]> Unknowns { get; private set; }
        public IReadOnlyList<ChestSpawnMapEntity> Chests { get; private set; }
        public IReadOnlyList<SignMapEntity> Signs { get; private set; }

        public MapFile(int id)
        {
            Properties = new MapFileProperties();
            Properties = Properties.WithMapID(id);

            Tiles = new Matrix<TileSpec>(1, 1, TileSpec.None);
            Warps = new Matrix<WarpMapEntity>(1, 1, null);
            GFX = new Dictionary<MapLayer, IReadOnlyMatrix<int>>();

            NPCSpawns = new List<NPCSpawnMapEntity>();
            Unknowns = new List<byte[]>();
            Chests = new List<ChestSpawnMapEntity>();
            Signs = new List<SignMapEntity>();
        }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                           IMapStringEncoderService mapStringEncoderService)
        {
            throw new System.NotImplementedException();
        }

        public void DeserializeFromByteArray(byte[] data,
            INumberEncoderService numberEncoderService,
            IMapStringEncoderService mapStringEncoderService)
        {
            throw new System.NotImplementedException();
        }
    }
}
