using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    [MappedType(BaseType = typeof(IMapFileSerializer))]
    [MappedType(BaseType = typeof(IMapDeserializer<IMapFile>))]
    public class MapFileSerializer : IMapFileSerializer
    {
        private const TileSpec DEFAULT_TILE = TileSpec.None;
        private static readonly WarpMapEntity DEFAULT_WARP = null;
        private const int DEFAULT_GFX = -1;

        private readonly IMapEntitySerializer<IMapFileProperties> _mapPropertiesSerializer;
        private readonly IMapEntitySerializer<NPCSpawnMapEntity> _npcSpawnMapEntitySerializer;
        private readonly IMapEntitySerializer<ChestSpawnMapEntity> _chestSpawnMapEntitySerializer;
        private readonly IMapEntitySerializer<WarpMapEntity> _warpMapEntitySerializer;
        private readonly IMapEntitySerializer<SignMapEntity> _signMapEntitySerializer;
        private readonly IMapEntitySerializer<UnknownMapEntity> _unknownMapEntitySerailizer;
        private readonly INumberEncoderService _numberEncoderService;

        public MapFileSerializer(IMapEntitySerializer<IMapFileProperties> mapPropertiesSerializer,
                                 IMapEntitySerializer<NPCSpawnMapEntity> npcSpawnMapEntitySerializer,
                                 IMapEntitySerializer<ChestSpawnMapEntity> chestSpawnMapEntitySerializer,
                                 IMapEntitySerializer<WarpMapEntity> warpMapEntitySerializer,
                                 IMapEntitySerializer<SignMapEntity> signMapEntitySerializer,
                                 IMapEntitySerializer<UnknownMapEntity> unknownMapEntitySerailizer,
                                 INumberEncoderService numberEncoderService)
        {
            _mapPropertiesSerializer = mapPropertiesSerializer;
            _npcSpawnMapEntitySerializer = npcSpawnMapEntitySerializer;
            _chestSpawnMapEntitySerializer = chestSpawnMapEntitySerializer;
            _warpMapEntitySerializer = warpMapEntitySerializer;
            _signMapEntitySerializer = signMapEntitySerializer;
            _unknownMapEntitySerailizer = unknownMapEntitySerailizer;
            _numberEncoderService = numberEncoderService;
        }

        public byte[] SerializeToByteArray(IMapFile mapFile, bool rewriteChecksum = true)
        {
            var ret = new List<byte>();

            ret.AddRange(_mapPropertiesSerializer.SerializeToByteArray(mapFile.Properties));
            ret.AddRange(WriteNPCSpawns(mapFile));
            ret.AddRange(WriteUnknowns(mapFile));
            ret.AddRange(WriteMapChests(mapFile));
            ret.AddRange(WriteTileSpecs(mapFile));
            ret.AddRange(WriteWarpTiles(mapFile));
            ret.AddRange(WriteGFXLayers(mapFile));
            ret.AddRange(WriteMapSigns(mapFile));

            var retBytes = ret.ToArray();
            if (rewriteChecksum)
            {
                var rid1 = _numberEncoderService.EncodeNumber(0, 2);
                var rid2 = _numberEncoderService.EncodeNumber(0, 2);

                Array.Copy(rid1, 0, retBytes, 3, 2);
                Array.Copy(rid2, 0, retBytes, 5, 2);

                var checksum = CRC32.Check(retBytes);
                var checksumBytes = _numberEncoderService.EncodeNumber((int)checksum, 4);
                Array.Copy(checksumBytes, 0, retBytes, 3, 4);
            }

            return retBytes;
        }

        public IMapFile DeserializeFromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var mapPropertiesData = new byte[MapFileProperties.DATA_SIZE];
                ms.Read(mapPropertiesData, 0, mapPropertiesData.Length);
                var properties = _mapPropertiesSerializer
                    .DeserializeFromByteArray(mapPropertiesData)
                    .WithFileSize(data.Length);

                var npcSpawns = ReadNPCSpawns(ms);
                var unknowns = ReadUnknowns(ms);
                var mapChests = ReadMapChests(ms);
                var (tileSpecs, emptyTileSpecRows) = ReadTileSpecs(ms, properties);
                var (warpTiles, emptyWarpRows) = ReadWarpTiles(ms, properties);
                var (gfxLayers, emptyLayers) = ReadGFXLayers(ms, properties);

                var mapSigns = new List<SignMapEntity>();
                if (ms.Position < ms.Length)
                    mapSigns = ReadMapSigns(ms);

                if (tileSpecs.Any(x => x.Any(y => y == TileSpec.SpikesTimed)))
                    properties = properties.WithHasTimedSpikes(true);

                return new MapFile()
                    .WithMapProperties(properties)
                    .WithNPCSpawns(npcSpawns)
                    .WithUnknowns(unknowns)
                    .WithChests(mapChests)
                    .WithTiles(tileSpecs, emptyTileSpecRows)
                    .WithWarps(warpTiles, emptyWarpRows)
                    .WithGFX(gfxLayers, emptyLayers)
                    .WithSigns(mapSigns);
            }
        }

        #region Helpers for Deserialization

        private List<NPCSpawnMapEntity> ReadNPCSpawns(MemoryStream ms)
        {
            var npcSpawns = new List<NPCSpawnMapEntity>();

            var collectionSize = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var npcSpawnData = new byte[NPCSpawnMapEntity.DATA_SIZE];
                ms.Read(npcSpawnData, 0, NPCSpawnMapEntity.DATA_SIZE);

                npcSpawns.Add(_npcSpawnMapEntitySerializer.DeserializeFromByteArray(npcSpawnData));
            }

            return npcSpawns;
        }

        private List<UnknownMapEntity> ReadUnknowns(MemoryStream ms)
        {
            var unknowns = new List<UnknownMapEntity>();

            var collectionSize = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var unknownData = new byte[UnknownMapEntity.DATA_SIZE];
                ms.Read(unknownData, 0, unknownData.Length);

                unknowns.Add(_unknownMapEntitySerailizer.DeserializeFromByteArray(unknownData));
            }

            return unknowns;
        }

        private List<ChestSpawnMapEntity> ReadMapChests(MemoryStream ms)
        {
            var chestSpawns = new List<ChestSpawnMapEntity>();

            var collectionSize = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var chestSpawnData = new byte[ChestSpawnMapEntity.DATA_SIZE];
                ms.Read(chestSpawnData, 0, ChestSpawnMapEntity.DATA_SIZE);

                chestSpawns.Add(_chestSpawnMapEntitySerializer.DeserializeFromByteArray(chestSpawnData));
            }

            return chestSpawns;
        }

        private (Matrix<TileSpec>, List<int>) ReadTileSpecs(MemoryStream ms, IMapFileProperties properties)
        {
            var tiles = new Matrix<TileSpec>(properties.Height + 1, properties.Width + 1, DEFAULT_TILE);
            var emptyTileRows = new List<int>();

            var numberOfTileRows = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < numberOfTileRows; ++i)
            {
                var y = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
                var numberOfTileColumns = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());

                if (numberOfTileColumns == 0)
                    emptyTileRows.Add(y);

                for (int j = 0; j < numberOfTileColumns; ++j)
                {
                    var x = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
                    var spec = (TileSpec)_numberEncoderService.DecodeNumber((byte)ms.ReadByte());

                    if (x <= properties.Width && y <= properties.Height)
                        tiles[y, x] = spec;
                }
            }

            return (tiles, emptyTileRows);
        }

        private (Matrix<WarpMapEntity>, List<int>) ReadWarpTiles(MemoryStream ms, IMapFileProperties properties)
        {
            var warps = new Matrix<WarpMapEntity>(properties.Height + 1, properties.Width + 1, DEFAULT_WARP);
            var emptyWarpRows = new List<int>();

            var numberOfWarpRows = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < numberOfWarpRows; ++i)
            {
                var y = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
                var numberOfWarpColumns = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());

                if (numberOfWarpColumns == 0)
                    emptyWarpRows.Add(y);

                for (int j = 0; j < numberOfWarpColumns; ++j)
                {
                    var rawWarpData = new byte[WarpMapEntity.DATA_SIZE];
                    ms.Read(rawWarpData, 0, rawWarpData.Length);

                    var warp = _warpMapEntitySerializer.DeserializeFromByteArray(rawWarpData).WithY(y);

                    if (warp.X <= properties.Width && warp.Y <= properties.Height)
                        warps[warp.Y, warp.X] = warp;
                }
            }

            return (warps, emptyWarpRows);
        }

        private (Dictionary<MapLayer, Matrix<int>>, Dictionary<MapLayer, List<int>>) ReadGFXLayers(MemoryStream ms, IMapFileProperties properties)
        {
            var gfx = new Dictionary<MapLayer, Matrix<int>>();
            var emptyRows = new Dictionary<MapLayer, List<int>>();

            var layers = (MapLayer[])Enum.GetValues(typeof(MapLayer));
            var twoByteBuffer = new byte[2];
            foreach (var layer in layers)
            {
                if (ms.Position == ms.Length)
                    break;

                gfx.Add(layer, new Matrix<int>(properties.Height + 1, properties.Width + 1, DEFAULT_GFX));

                var numberOfRowsThisLayer = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
                for (int i = 0; i < numberOfRowsThisLayer; ++i)
                {
                    var y = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
                    var numberOfColsThisLayer = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());

                    if (numberOfColsThisLayer == 0)
                    {
                        if (!emptyRows.ContainsKey(layer))
                            emptyRows.Add(layer, new List<int>());

                        emptyRows[layer].Add(y);
                    }

                    for (int j = 0; j < numberOfColsThisLayer; ++j)
                    {
                        var x = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());

                        ms.Read(twoByteBuffer, 0, twoByteBuffer.Length);
                        var gfxID = (ushort)_numberEncoderService.DecodeNumber(twoByteBuffer);

                        if (x <= properties.Width && y <= properties.Height)
                            gfx[layer][y, x] = gfxID;
                    }
                }
            }

            return (gfx, emptyRows);
        }

        private List<SignMapEntity> ReadMapSigns(MemoryStream ms)
        {
            var signs = new List<SignMapEntity>();

            var collectionSize = _numberEncoderService.DecodeNumber((byte)ms.ReadByte());
            for (int i = 0; i < collectionSize; ++i)
            {
                var variableLength = GetVariableSignLength(ms);

                var signData = new byte[variableLength + SignMapEntity.DATA_SIZE];
                ms.Read(signData, 0, signData.Length);

                signs.Add(_signMapEntitySerializer.DeserializeFromByteArray(signData));
            }

            return signs;
        }

        private int GetVariableSignLength(MemoryStream ms)
        {
            var messageLengthRaw = new byte[2];

            ms.Seek(2, SeekOrigin.Current); //skip sign X and Y coordinates
            ms.Read(messageLengthRaw, 0, 2); //read two-byte length
            ms.Seek(-4, SeekOrigin.Current); //skip back to beginning

            return _numberEncoderService.DecodeNumber(messageLengthRaw);
        }

        #endregion

        #region Helpers for Serialization

        private List<byte> WriteNPCSpawns(IMapFile mapFile)
        {
            var ret = new List<byte>();
            ret.AddRange(_numberEncoderService.EncodeNumber(mapFile.NPCSpawns.Count, 1));
            foreach (var spawn in mapFile.NPCSpawns)
                ret.AddRange(_npcSpawnMapEntitySerializer.SerializeToByteArray(spawn));
            return ret;
        }

        private List<byte> WriteUnknowns(IMapFile mapFile)
        {
            var ret = new List<byte>();

            ret.AddRange(_numberEncoderService.EncodeNumber(mapFile.Unknowns.Count, 1));
            foreach (var unknown in mapFile.Unknowns)
                ret.AddRange(_unknownMapEntitySerailizer.SerializeToByteArray(unknown));

            return ret;
        }

        private List<byte> WriteMapChests(IMapFile mapFile)
        {
            var ret = new List<byte>();
            ret.AddRange(_numberEncoderService.EncodeNumber(mapFile.Chests.Count, 1));
            foreach (var chest in mapFile.Chests)
                ret.AddRange(_chestSpawnMapEntitySerializer.SerializeToByteArray(chest));
            return ret;
        }

        private List<byte> WriteTileSpecs(IMapFile mapFile)
        {
            var ret = new List<byte>();

            var tileRows = mapFile.Tiles
                .Select((row, i) => (row, i))
                .Where(rowList => rowList.row.Any(item => item != DEFAULT_TILE))
                .Concat<(IList<TileSpec> EntityItems, int Y)>(mapFile.EmptyTileRows.Select(rowNdx => ((IList<TileSpec>)new List<TileSpec>(), rowNdx)))
                .ToList();

            ret.AddRange(_numberEncoderService.EncodeNumber(tileRows.Count, 1));
            foreach (var row in tileRows)
            {
                var entityItems = row.EntityItems
                    .Select((item, i) => new { Value = item, X = i })
                    .Where(item => item.Value != DEFAULT_TILE)
                    .ToList();

                ret.AddRange(_numberEncoderService.EncodeNumber(row.Y, 1));
                ret.AddRange(_numberEncoderService.EncodeNumber(entityItems.Count, 1));
                foreach (var item in entityItems)
                {
                    ret.AddRange(_numberEncoderService.EncodeNumber(item.X, 1));
                    ret.AddRange(_numberEncoderService.EncodeNumber((byte)item.Value, 1));
                }
            }

            return ret;
        }

        private List<byte> WriteWarpTiles(IMapFile mapFile)
        {
            var ret = new List<byte>();

            var warpRows = mapFile.Warps
                .Select((row, i) => (row, i))
                .Where(rowList => rowList.row.Any(item => item != DEFAULT_WARP))
                .Concat<(IList<WarpMapEntity> EntityItems, int Y)>(mapFile.EmptyWarpRows.Select(rowNdx => ((IList<WarpMapEntity>)new List<WarpMapEntity>(), rowNdx)))
                .ToList();

            ret.AddRange(_numberEncoderService.EncodeNumber(warpRows.Count, 1));
            foreach (var row in warpRows)
            {
                var entityItems = row.EntityItems
                    .Select((item, i) => new { Value = item, X = i })
                    .Where(item => item.Value != DEFAULT_WARP)
                    .ToList();

                ret.AddRange(_numberEncoderService.EncodeNumber(row.Y, 1));
                ret.AddRange(_numberEncoderService.EncodeNumber(entityItems.Count, 1));
                foreach (var item in entityItems)
                    ret.AddRange(_warpMapEntitySerializer.SerializeToByteArray(item.Value));
            }
            return ret;
        }

        private List<byte> WriteGFXLayers(IMapFile mapFile)
        {
            var ret = new List<byte>();

            foreach (var layer in mapFile.GFX.Keys)
            {
                var gfxRowsForLayer = mapFile.GFX[layer]
                    .Select((row, i) => new { EntityItems = row, Y = i })
                    .Where(rowList => rowList.EntityItems.Any(item => item != DEFAULT_GFX))
                    .ToList();

                var rowsForLayerCount = gfxRowsForLayer.Count + (mapFile.EmptyGFXRows.ContainsKey(layer) ? mapFile.EmptyGFXRows[layer].Count : 0);
                ret.AddRange(_numberEncoderService.EncodeNumber(rowsForLayerCount, 1));

                foreach (var row in gfxRowsForLayer)
                {
                    var entityItems = row.EntityItems
                        .Select((item, i) => new { Value = item, X = i })
                        .Where(item => item.Value != DEFAULT_GFX)
                        .ToList();

                    ret.AddRange(_numberEncoderService.EncodeNumber(row.Y, 1));
                    ret.AddRange(_numberEncoderService.EncodeNumber(entityItems.Count, 1));
                    foreach (var item in entityItems)
                    {
                        ret.AddRange(_numberEncoderService.EncodeNumber(item.X, 1));
                        ret.AddRange(_numberEncoderService.EncodeNumber(item.Value, 2));
                    }
                }

                if (mapFile.EmptyGFXRows.ContainsKey(layer))
                {
                    foreach (var emptyLayer in mapFile.EmptyGFXRows[layer])
                    {
                        ret.AddRange(_numberEncoderService.EncodeNumber(emptyLayer, 1));
                        ret.AddRange(_numberEncoderService.EncodeNumber(0, 1));
                    }
                }
            }
            return ret;
        }

        private List<byte> WriteMapSigns(IMapFile mapFile)
        {
            var ret = new List<byte>();

            ret.AddRange(_numberEncoderService.EncodeNumber(mapFile.Signs.Count, 1));
            foreach (var sign in mapFile.Signs)
                ret.AddRange(_signMapEntitySerializer.SerializeToByteArray(sign));
            return ret;
        }

        #endregion
    }
}
