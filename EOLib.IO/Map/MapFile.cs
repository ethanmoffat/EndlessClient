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

        public MapFile()
            : this(new MapFileProperties(),
            Matrix<TileSpec>.Empty,
            Matrix<WarpMapEntity>.Empty,
            new Dictionary<MapLayer, Matrix<int>>(),
            new List<NPCSpawnMapEntity>(),
            new List<byte[]>(),
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
            List<byte[]> unknowns,
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

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                           IMapStringEncoderService mapStringEncoderService)
        {
            var ret = new List<byte>();

            ISerializer<IMapFileProperties> serializer =
                new MapPropertiesSerializer(numberEncoderService, mapStringEncoderService);

            ret.AddRange(serializer.SerializeToByteArray(Properties));

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
            ISerializer<IMapFileProperties> serializer =
                new MapPropertiesSerializer(numberEncoderService, mapStringEncoderService);

            using (var ms = new MemoryStream(data))
            {
                var mapPropertiesData = new byte[MapFileProperties.DATA_SIZE];
                ms.Read(mapPropertiesData, 0, mapPropertiesData.Length);
                Properties = serializer
                    .DeserializeFromByteArray(mapPropertiesData)
                    .WithFileSize(data.Length);

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

                for (int j = 0; j < numberOfTileColumns; ++j)
                {
                    var x = nes.DecodeNumber((byte)ms.ReadByte());
                    var spec = (TileSpec)nes.DecodeNumber((byte)ms.ReadByte());
                    if (spec == TileSpec.SpikesTimed && !Properties.HasTimedSpikes)
                        Properties = Properties.WithHasTimedSpikes(true);

                    if (x <= Properties.Width && y <= Properties.Height)
                        _mutableTiles[y, x] = spec;
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

                for (int j = 0; j < numberOfWarpColumns; ++j)
                {
                    var rawWarpData = new byte[serializer.DataSize];
                    ms.Read(rawWarpData, 0, rawWarpData.Length);

                    var warp = serializer.DeserializeFromByteArray(rawWarpData);
                    warp.Y = y;

                    if (warp.X <= Properties.Width && warp.Y <= Properties.Height)
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

                        if (x <= Properties.Width && y <= Properties.Height)
                            _mutableGFX[layer][y, x] = gfxID;
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
            var tileRows = _mutableTiles
                .Select((row, i) => new {EntityItems = row, Y = i})
                .Where(rowList => rowList.EntityItems.Any(item => item != TileSpec.None))
                .ToList();

            ret.AddRange(nes.EncodeNumber(tileRows.Count, 1));
            foreach (var row in tileRows)
            {
                var entityItems = row.EntityItems
                    .Select((item, i) => new { Value = item, X = i })
                    .Where(item => item.Value != TileSpec.None)
                    .ToList();

                ret.AddRange(nes.EncodeNumber(row.Y, 1));
                ret.AddRange(nes.EncodeNumber(entityItems.Count, 1));
                foreach (var item in entityItems)
                {
                    ret.AddRange(nes.EncodeNumber(item.X, 1));
                    ret.AddRange(nes.EncodeNumber((byte)item.Value, 1));
                }
            }
        }

        private void WriteWarpTiles(List<byte> ret, INumberEncoderService nes)
        {
            IMapEntitySerializer<WarpMapEntity> serializer = new WarpMapEntitySerializer(nes);

            var warpRows = _mutableWarps
                .Select((row, i) => new { EntityItems = row, Y = i })
                .Where(rowList => rowList.EntityItems.Any(item => item != null)) //todo: use optional for default instead of null
                .ToList();

            ret.AddRange(nes.EncodeNumber(warpRows.Count, 1));
            foreach (var row in warpRows)
            {
                var entityItems = row.EntityItems
                    .Select((item, i) => new { Value = item, X = i })
                    .Where(item => item.Value != null)
                    .ToList();

                ret.AddRange(nes.EncodeNumber(row.Y, 1));
                ret.AddRange(nes.EncodeNumber(entityItems.Count, 1));
                foreach (var item in entityItems)
                    ret.AddRange(serializer.SerializeToByteArray(item.Value));
            }
        }

        private void WriteGFXLayers(List<byte> ret, INumberEncoderService nes)
        {
            foreach (var layer in _mutableGFX.Keys)
            {
                var gfxRowsForLayer = _mutableGFX[layer]
                    .Select((row, i) => new { EntityItems = row, Y = i })
                    .Where(rowList => rowList.EntityItems.Any(item => item != -1))
                    .ToList();

                ret.AddRange(nes.EncodeNumber(gfxRowsForLayer.Count, 1));
                foreach (var row in gfxRowsForLayer)
                {
                    var entityItems = row.EntityItems
                        .Select((item, i) => new { Value = item, X = i })
                        .Where(item => item.Value != -1)
                        .ToList();

                    ret.AddRange(nes.EncodeNumber(row.Y, 1));
                    ret.AddRange(nes.EncodeNumber(entityItems.Count, 1));
                    foreach (var item in entityItems)
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

        #endregion

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
