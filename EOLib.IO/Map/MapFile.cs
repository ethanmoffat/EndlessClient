// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public class MapFile : IMapFile
    {
        public IMapFileProperties Properties { get; private set; }

        public IReadOnlyMatrix<TileSpec> Tiles { get { return _mutableTiles; } }
        public IReadOnlyMatrix<WarpMapEntity> Warps { get { return _mutableWarps; } }

        public IReadOnlyDictionary<MapLayer, IReadOnlyMatrix<int>> GFX { get { return _mutableGFX.ToDictionary(k => k.Key, v => (IReadOnlyMatrix<int>)v.Value); } }

        private Matrix<TileSpec> _mutableTiles;
        private Matrix<WarpMapEntity> _mutableWarps;
        private Dictionary<MapLayer, Matrix<int>> _mutableGFX;

        public IReadOnlyList<NPCSpawnMapEntity> NPCSpawns { get; private set; }
        public IReadOnlyList<byte[]> Unknowns { get; private set; }
        public IReadOnlyList<ChestSpawnMapEntity> Chests { get; private set; }
        public IReadOnlyList<SignMapEntity> Signs { get; private set; }

        public MapFile(int id)
        {
            Properties = new MapFileProperties();
            Properties = Properties.WithMapID(id);
            
            ResetCollections();
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
            var mapPropertiesData = data.Take(MapFileProperties.DATA_SIZE).ToArray();
            Properties = Properties
                .WithFileSize(data.Length)
                .DeserializeFromByteArray(mapPropertiesData, numberEncoderService, mapStringEncoderService);

            ResetCollections();
        }

        private void ResetCollections()
        {
            _mutableTiles = new Matrix<TileSpec>(Properties.Width, Properties.Height, TileSpec.None);
            _mutableWarps = new Matrix<WarpMapEntity>(Properties.Width, Properties.Height, null);
            _mutableGFX = new Dictionary<MapLayer, Matrix<int>>();
            foreach (var layer in (MapLayer[]) Enum.GetValues(typeof(MapLayer)))
                _mutableGFX.Add(layer, new Matrix<int>(Properties.Width, Properties.Height, -1));

            NPCSpawns = new List<NPCSpawnMapEntity>();
            Unknowns = new List<byte[]>();
            Chests = new List<ChestSpawnMapEntity>();
            Signs = new List<SignMapEntity>();
        }

        #region Helpers for Serialization

        private List<MapEntityRow<T>> GetSerializationCollection<T>(Matrix<T> m, T fillValue)
        {
            var retList = new List<MapEntityRow<T>>();

            for (int r = 0; r < m.Rows; ++r)
            {
                var nextEntityRow = new MapEntityRow<T> {Y = r};

                var row = m.GetRow(r);
                if (row.All(x => x.Equals(fillValue)))
                    continue;

                var mapEntityItems = row.Select((val, x) => new MapEntityItem<T> {X = x, Value = val})
                                        .Where(x => !x.Value.Equals(fillValue));

                nextEntityRow.EntityItems.AddRange(mapEntityItems);
                retList.Add(nextEntityRow);
            }

            return retList;
        }

        private class MapEntityRow<T>
        {
            public int Y { get; set; }

            public List<MapEntityItem<T>> EntityItems { get; private set; }

            public MapEntityRow()
            {
                EntityItems = new List<MapEntityItem<T>>();
            }
        }

        private class MapEntityItem<T>
        {
            public int X { get; set; }

            public T Value { get; set; }
        }

        #endregion
    }
}
