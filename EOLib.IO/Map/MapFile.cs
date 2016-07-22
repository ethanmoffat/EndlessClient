// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public class MapFile : IMapFile
    {
        public IMapFileProperties Properties { get; private set; }

        public IReadOnlyMatrix<TileSpec> Tiles { get { return _mutableTiles; } }
        
        public IReadOnlyMatrix<WarpMapEntity> Warps { get { return _mutableWarps; } }

        public IReadOnlyDictionary<MapLayer, IReadOnlyMatrix<int>> GFX
        {
            get { return _mutableGFX.ToDictionary(k => k.Key, v => (IReadOnlyMatrix<int>) v.Value); }
        }

        public IReadOnlyList<NPCSpawnMapEntity> NPCSpawns { get { return _mutableNPCSpawns; } }
        public IReadOnlyList<byte[]> Unknowns { get { return _mutableUnknowns; } }
        public IReadOnlyList<ChestSpawnMapEntity> Chests { get { return _mutableChestSpawns; } }
        public IReadOnlyList<SignMapEntity> Signs { get { return _mutableSigns; } }

        private Matrix<TileSpec> _mutableTiles;
        private Matrix<WarpMapEntity> _mutableWarps;
        private Dictionary<MapLayer, Matrix<int>> _mutableGFX;
        private List<NPCSpawnMapEntity> _mutableNPCSpawns;
        private List<byte[]> _mutableUnknowns;
        private List<ChestSpawnMapEntity> _mutableChestSpawns;
        private List<SignMapEntity> _mutableSigns;

        public MapFile(int id)
        {
            Properties = new MapFileProperties();
            Properties = Properties.WithMapID(id);
            
            ResetCollections();
        }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                           IMapStringEncoderService mapStringEncoderService)
        {
            throw new NotImplementedException();
        }

        public void DeserializeFromByteArray(byte[] data,
                                             INumberEncoderService numberEncoderService,
                                             IMapStringEncoderService mapStringEncoderService)
        {
            using (var ms = new MemoryStream(data))
            {
                var mapPropertiesData = new byte[MapFileProperties.DATA_SIZE];
                ms.Read(mapPropertiesData, 0, mapPropertiesData.Length);
                Properties = Properties
                    .WithFileSize(data.Length)
                    .DeserializeFromByteArray(mapPropertiesData, numberEncoderService, mapStringEncoderService);

                ResetCollections();
                ReadNPCSpawns(ms, numberEncoderService, mapStringEncoderService);
                ReadUnknowns(ms, numberEncoderService);
                ReadMapChests(ms, numberEncoderService, mapStringEncoderService);
                ReadTileSpecs(ms, numberEncoderService);
                ReadWarpTiles(ms, numberEncoderService, mapStringEncoderService);
                ReadGFXLayers(ms, numberEncoderService);

                if (ms.Position == ms.Length)
                    return;

                ReadMapSigns(ms, numberEncoderService, mapStringEncoderService);
            }
        }

        private void ResetCollections()
        {
            _mutableTiles = new Matrix<TileSpec>(Properties.Width, Properties.Height, TileSpec.None);
            _mutableWarps = new Matrix<WarpMapEntity>(Properties.Width, Properties.Height, null);
            _mutableGFX = new Dictionary<MapLayer, Matrix<int>>();
            foreach (var layer in (MapLayer[]) Enum.GetValues(typeof(MapLayer)))
                _mutableGFX.Add(layer, new Matrix<int>(Properties.Width, Properties.Height, -1));

            _mutableNPCSpawns = new List<NPCSpawnMapEntity>();
            _mutableUnknowns = new List<byte[]>();
            _mutableChestSpawns = new List<ChestSpawnMapEntity>();
            _mutableSigns = new List<SignMapEntity>();
        }

        #region Helpers for Deserialization

        private void ReadNPCSpawns(MemoryStream ms, INumberEncoderService nes, IMapStringEncoderService ses)
        {
            var collectionSize = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var npcSpawn = new NPCSpawnMapEntity();
                
                var npcSpawnData = new byte[npcSpawn.DataSize];
                ms.Read(npcSpawnData, 0, npcSpawn.DataSize);

                npcSpawn.DeserializeFromByteArray(npcSpawnData, nes, ses);
                _mutableNPCSpawns.Add(npcSpawn);
            }
        }

        private void ReadUnknowns(MemoryStream ms, INumberEncoderService nes)
        {
            var collectionSize = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var unknown = new byte[4];
                ms.Read(unknown, 0, unknown.Length);
                _mutableUnknowns.Add(unknown);
            }
        }

        private void ReadMapChests(MemoryStream ms, INumberEncoderService nes, IMapStringEncoderService ses)
        {
            var collectionSize = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var chest = new ChestSpawnMapEntity();

                var npcSpawnData = new byte[chest.DataSize];
                ms.Read(npcSpawnData, 0, chest.DataSize);

                chest.DeserializeFromByteArray(npcSpawnData, nes, ses);
                _mutableChestSpawns.Add(chest);
            }
        }

        private void ReadTileSpecs(MemoryStream ms, INumberEncoderService nes)
        {
            var numberOfTileRows = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < numberOfTileRows; ++i)
            {
                var y = nes.DecodeNumber((byte)ms.ReadByte());
                var numberOfTileColumns = nes.DecodeNumber((byte)ms.ReadByte());

                for (int j = 0; j < numberOfTileColumns; ++j)
                {
                    var x = nes.DecodeNumber((byte)ms.ReadByte());
                    var spec = (TileSpec)nes.DecodeNumber((byte)ms.ReadByte());
                    if (spec == TileSpec.SpikesTimed && !Properties.HasTimedSpikes)
                        Properties = Properties.WithHasTimedSpikes(true);

                    _mutableTiles[y, x] = spec;
                }
            }
        }

        private void ReadWarpTiles(MemoryStream ms, INumberEncoderService nes, IMapStringEncoderService ses)
        {
            var numberOfWarpRows = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < numberOfWarpRows; ++i)
            {
                var y = nes.DecodeNumber((byte)ms.ReadByte());
                var numberOfWarpColumns = nes.DecodeNumber((byte)ms.ReadByte());

                for (int j = 0; j < numberOfWarpColumns; ++j)
                {
                    var warp = new WarpMapEntity(y);
                    
                    var rawWarpData = new byte[warp.DataSize];
                    ms.Read(rawWarpData, 0, rawWarpData.Length);

                    warp.DeserializeFromByteArray(rawWarpData, nes, ses);

                    if (warp.Y <= Properties.Height && warp.X <= Properties.Width)
                        _mutableWarps[warp.Y, warp.X] = warp;
                }
            }
        }

        private void ReadGFXLayers(MemoryStream ms, INumberEncoderService nes)
        {
            var layers = (MapLayer[]) Enum.GetValues(typeof(MapLayer));
            var twoByteBuffer = new byte[2];
            foreach (var layer in layers)
            {
                if (ms.Position == ms.Length)
                    break;

                var numberOfRowsThisLayer = nes.DecodeNumber((byte) ms.ReadByte());
                for (int i = 0; i < numberOfRowsThisLayer; ++i)
                {
                    var y = nes.DecodeNumber((byte) ms.ReadByte());
                    var numberOfColsThisLayer = nes.DecodeNumber((byte) ms.ReadByte());

                    for (int j = 0; j < numberOfColsThisLayer; ++j)
                    {
                        var x = nes.DecodeNumber((byte) ms.ReadByte());

                        ms.Read(twoByteBuffer, 0, twoByteBuffer.Length);
                        var gfxID = (ushort) nes.DecodeNumber(twoByteBuffer);

                        if (y <= Properties.Height && x <= Properties.Width)
                            _mutableGFX[layer][y, x] = gfxID;
                    }
                }
            }
        }

        private void ReadMapSigns(MemoryStream ms, INumberEncoderService nes, IMapStringEncoderService ses)
        {
            var collectionSize = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var variableLength = GetVariableSignLength(ms, nes);

                var sign = new SignMapEntity();
                var signData = new byte[variableLength + sign.DataSize];
                ms.Read(signData, 0, signData.Length);

                sign.DeserializeFromByteArray(signData, nes, ses);
                _mutableSigns.Add(sign);
            }
        }

        private static int GetVariableSignLength(MemoryStream ms, INumberEncoderService nes)
        {
            var messageLengthRaw = new byte[2];

            ms.Seek(2, SeekOrigin.Current); //skip sign X and Y coordinates
            ms.Read(messageLengthRaw, 0, 2); //read two-byte length
            ms.Seek(-4, SeekOrigin.Current); //skip back to beginning

            return nes.DecodeNumber(messageLengthRaw);
        }

        #endregion

        #region Helper classes for Serialization

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
