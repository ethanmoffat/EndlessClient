// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;

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

        public IReadOnlyList<MapEntityRow<TileSpec>> TileRows { get { return _mutableTileRows; } }
        public IReadOnlyList<MapEntityRow<WarpMapEntity>> WarpRows { get { return _mutableWarpRows; } }
        public IReadOnlyDictionary<MapLayer, IReadOnlyList<MapEntityRow<int>>> GFXRows
        {
            get { return _mutableGFXRows.ToDictionary(k => k.Key, v => (IReadOnlyList<MapEntityRow<int>>) v.Value); }
        }

        private List<MapEntityRow<TileSpec>> _mutableTileRows;
        private List<MapEntityRow<WarpMapEntity>> _mutableWarpRows;
        private Dictionary<MapLayer, List<MapEntityRow<int>>> _mutableGFXRows;

        public MapFile(int id)
        {
            Properties = new MapFileProperties();
            Properties = Properties.WithMapID(id);

            ResetCollections();
        }

        #region File Modifications (to support BatchMap)

        public void RemoveNPCSpawn(NPCSpawnMapEntity spawn)
        {
            _mutableNPCSpawns.Remove(spawn);
        }

        public void RemoveChestSpawn(ChestSpawnMapEntity spawn)
        {
            _mutableChestSpawns.Remove(spawn);
        }

        public void RemoveTileAt(int x, int y)
        {
            _mutableTiles[y, x] = TileSpec.None;

            var tileRow = _mutableTileRows.Single(w => w.Y == y);
            tileRow.EntityItems.Remove(tileRow.EntityItems.Single(w => w.X == x));
        }

        public void RemoveWarp(WarpMapEntity warp)
        {
            RemoveWarpAt(warp.X, warp.Y);
        }

        public void RemoveWarpAt(int x, int y)
        {
            _mutableWarps[y, x] = null;

            var warpRow = _mutableWarpRows.Single(w => w.Y == y);
            warpRow.EntityItems.Remove(warpRow.EntityItems.Single(w => w.X == x));
        }

        #endregion

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                           IMapStringEncoderService mapStringEncoderService)
        {
            var ret = new List<byte>();

            ret.AddRange(Properties.SerializeToByteArray(numberEncoderService, mapStringEncoderService));
            WriteNPCSpawns(ret, numberEncoderService);
            WriteUnknowns(ret, numberEncoderService);
            WriteMapChests(ret, numberEncoderService);
            WriteTileSpecs(ret, numberEncoderService);
            WriteWarpTiles(ret, numberEncoderService);
            WriteGFXLayers(ret, numberEncoderService);
            WriteMapSigns(ret, numberEncoderService, mapStringEncoderService);

            return ret.ToArray();
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
                ReadNPCSpawns(ms, numberEncoderService);
                ReadUnknowns(ms, numberEncoderService);
                ReadMapChests(ms, numberEncoderService);
                ReadTileSpecs(ms, numberEncoderService);
                ReadWarpTiles(ms, numberEncoderService);
                ReadGFXLayers(ms, numberEncoderService);

                if (ms.Position == ms.Length)
                    return;

                ReadMapSigns(ms, numberEncoderService, mapStringEncoderService);
            }
        }

        private void ResetCollections()
        {
            _mutableTiles = new Matrix<TileSpec>(Properties.Height + 1, Properties.Width + 1, TileSpec.None);
            _mutableWarps = new Matrix<WarpMapEntity>(Properties.Height + 1, Properties.Width + 1, null);
            _mutableGFX = new Dictionary<MapLayer, Matrix<int>>();
            foreach (var layer in (MapLayer[]) Enum.GetValues(typeof(MapLayer)))
                _mutableGFX.Add(layer, new Matrix<int>(Properties.Height + 1, Properties.Width + 1, -1));
            _mutableNPCSpawns = new List<NPCSpawnMapEntity>();
            _mutableUnknowns = new List<byte[]>();
            _mutableChestSpawns = new List<ChestSpawnMapEntity>();
            _mutableSigns = new List<SignMapEntity>();

            _mutableTileRows = new List<MapEntityRow<TileSpec>>();
            _mutableWarpRows = new List<MapEntityRow<WarpMapEntity>>();
            _mutableGFXRows = new Dictionary<MapLayer, List<MapEntityRow<int>>>();
            foreach (var layer in (MapLayer[]) Enum.GetValues(typeof(MapLayer)))
                _mutableGFXRows.Add(layer, new List<MapEntityRow<int>>());
        }

        #region Helpers for Deserialization

        private void ReadNPCSpawns(MemoryStream ms, INumberEncoderService nes)
        {
            IMapEntitySerializer<NPCSpawnMapEntity> serializer = new NPCSpawnMapEntitySerializer(nes);

            var collectionSize = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var npcSpawnData = new byte[serializer.DataSize];
                ms.Read(npcSpawnData, 0, serializer.DataSize);

                _mutableNPCSpawns.Add(serializer.DeserializeFromByteArray(npcSpawnData));
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

        private void ReadMapChests(MemoryStream ms, INumberEncoderService nes)
        {
            IMapEntitySerializer<ChestSpawnMapEntity> serializer = new ChestSpawnMapEntitySerializer(nes);

            var collectionSize = nes.DecodeNumber((byte) ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var chestSpawnData = new byte[serializer.DataSize];
                ms.Read(chestSpawnData, 0, serializer.DataSize);

                _mutableChestSpawns.Add(serializer.DeserializeFromByteArray(chestSpawnData));
            }
        }

        private void ReadTileSpecs(MemoryStream ms, INumberEncoderService nes)
        {
            var numberOfTileRows = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < numberOfTileRows; ++i)
            {
                var y = nes.DecodeNumber((byte)ms.ReadByte());
                var numberOfTileColumns = nes.DecodeNumber((byte)ms.ReadByte());

                _mutableTileRows.Add(new MapEntityRow<TileSpec> {Y = y});
                for (int j = 0; j < numberOfTileColumns; ++j)
                {
                    var x = nes.DecodeNumber((byte)ms.ReadByte());
                    var spec = (TileSpec)nes.DecodeNumber((byte)ms.ReadByte());
                    if (spec == TileSpec.SpikesTimed && !Properties.HasTimedSpikes)
                        Properties = Properties.WithHasTimedSpikes(true);

                    if (x <= Properties.Width && y <= Properties.Height)
                        _mutableTiles[y, x] = spec;
                    _mutableTileRows.Last().EntityItems.Add(new MapEntityCol<TileSpec> {X = x, Value = spec});
                }
            }
        }

        private void ReadWarpTiles(MemoryStream ms, INumberEncoderService nes)
        {
            IMapEntitySerializer<WarpMapEntity> serializer = new WarpMapEntitySerializer(nes);

            var numberOfWarpRows = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < numberOfWarpRows; ++i)
            {
                var y = nes.DecodeNumber((byte)ms.ReadByte());
                var numberOfWarpColumns = nes.DecodeNumber((byte)ms.ReadByte());

                _mutableWarpRows.Add(new MapEntityRow<WarpMapEntity> {Y = y});
                for (int j = 0; j < numberOfWarpColumns; ++j)
                {
                    var rawWarpData = new byte[serializer.DataSize];
                    ms.Read(rawWarpData, 0, rawWarpData.Length);

                    var warp = serializer.DeserializeFromByteArray(rawWarpData);
                    warp.Y = y;

                    if (warp.X <= Properties.Width && warp.Y <= Properties.Height)
                        _mutableWarps[warp.Y, warp.X] = warp;
                    _mutableWarpRows.Last().EntityItems.Add(new MapEntityCol<WarpMapEntity> {X = warp.X, Value = warp});
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

                    _mutableGFXRows[layer].Add(new MapEntityRow<int> {Y = y});
                    for (int j = 0; j < numberOfColsThisLayer; ++j)
                    {
                        var x = nes.DecodeNumber((byte) ms.ReadByte());

                        ms.Read(twoByteBuffer, 0, twoByteBuffer.Length);
                        var gfxID = (ushort) nes.DecodeNumber(twoByteBuffer);

                        if (x <= Properties.Width && y <= Properties.Height)
                            _mutableGFX[layer][y, x] = gfxID;
                        _mutableGFXRows[layer].Last().EntityItems.Add(new MapEntityCol<int> {X = x, Value = gfxID});
                    }
                }
            }
        }

        private void ReadMapSigns(MemoryStream ms, INumberEncoderService nes, IMapStringEncoderService ses)
        {
            IMapEntitySerializer<SignMapEntity> serializer = new SignMapEntitySerializer(nes, ses);

            var collectionSize = nes.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var variableLength = GetVariableSignLength(ms, nes);

                var signData = new byte[variableLength + serializer.DataSize];
                ms.Read(signData, 0, signData.Length);

                _mutableSigns.Add(serializer.DeserializeFromByteArray(signData));
            }
        }

        private static int GetVariableSignLength(MemoryStream ms, INumberEncoderService nes)
        {
            var messageLengthRaw = new byte[2];

            ms.Seek(2, SeekOrigin.Current); //skip sign X and Y coordinates
            ms.Read(messageLengthRaw, 0, 2); //read two-byte length
            ms.Seek(-4, SeekOrigin.Current); //skip back to beginning

            return nes.DecodeNumber(messageLengthRaw) - 1;
        }

        #endregion

        #region Helpers for Serialization

        private void WriteNPCSpawns(List<byte> ret, INumberEncoderService nes)
        {
            IMapEntitySerializer<NPCSpawnMapEntity> serializer = new NPCSpawnMapEntitySerializer(nes);
            ret.AddRange(nes.EncodeNumber(NPCSpawns.Count, 1));
            foreach (var spawn in NPCSpawns)
                ret.AddRange(serializer.SerializeToByteArray(spawn));
        }

        private void WriteUnknowns(List<byte> ret, INumberEncoderService nes)
        {
            ret.AddRange(nes.EncodeNumber(Unknowns.Count, 1));
            foreach (var unknown in Unknowns)
                ret.AddRange(unknown);
        }

        private void WriteMapChests(List<byte> ret, INumberEncoderService nes)
        {
            IMapEntitySerializer<ChestSpawnMapEntity> serializer = new ChestSpawnMapEntitySerializer(nes);
            ret.AddRange(nes.EncodeNumber(Chests.Count, 1));
            foreach (var chest in Chests)
                ret.AddRange(serializer.SerializeToByteArray(chest));
        }

        private void WriteTileSpecs(List<byte> ret, INumberEncoderService nes)
        {
            ret.AddRange(nes.EncodeNumber(TileRows.Count, 1));
            foreach (var row in TileRows)
            {
                ret.AddRange(nes.EncodeNumber(row.Y, 1));
                ret.AddRange(nes.EncodeNumber(row.EntityItems.Count, 1));
                foreach (var item in row.EntityItems)
                {
                    ret.AddRange(nes.EncodeNumber(item.X, 1));
                    ret.AddRange(nes.EncodeNumber((byte)item.Value, 1));
                }
            }
        }

        private void WriteWarpTiles(List<byte> ret, INumberEncoderService nes)
        {
            IMapEntitySerializer<WarpMapEntity> serializer = new WarpMapEntitySerializer(nes);
            ret.AddRange(nes.EncodeNumber(WarpRows.Count, 1));
            foreach (var row in WarpRows)
            {
                ret.AddRange(nes.EncodeNumber(row.Y, 1));
                ret.AddRange(nes.EncodeNumber(row.EntityItems.Count, 1));
                foreach (var item in row.EntityItems)
                    ret.AddRange(serializer.SerializeToByteArray(item.Value));
            }
        }

        private void WriteGFXLayers(List<byte> ret, INumberEncoderService nes)
        {
            foreach (var layer in _mutableGFX.Keys)
            {
                ret.AddRange(nes.EncodeNumber(GFXRows[layer].Count, 1));
                foreach (var row in GFXRows[layer])
                {
                    ret.AddRange(nes.EncodeNumber(row.Y, 1));
                    ret.AddRange(nes.EncodeNumber(row.EntityItems.Count, 1));
                    foreach (var item in row.EntityItems)
                    {
                        ret.AddRange(nes.EncodeNumber(item.X, 1));
                        ret.AddRange(nes.EncodeNumber(item.Value, 2));
                    }
                }
            }
        }

        private void WriteMapSigns(List<byte> ret, INumberEncoderService nes, IMapStringEncoderService ses)
        {
            IMapEntitySerializer<SignMapEntity> serializer = new SignMapEntitySerializer(nes, ses);
            ret.AddRange(nes.EncodeNumber(Signs.Count, 1));
            foreach (var sign in Signs)
                ret.AddRange(serializer.SerializeToByteArray(sign));
        }

        public class MapEntityRow<T>
        {
            public int Y { get; set; }

            public List<MapEntityCol<T>> EntityItems { get; private set; }

            public MapEntityRow()
            {
                EntityItems = new List<MapEntityCol<T>>();
            }
        }

        public class MapEntityCol<T>
        {
            public int X { get; set; }

            public T Value { get; set; }
        }

        #endregion
    }
}
