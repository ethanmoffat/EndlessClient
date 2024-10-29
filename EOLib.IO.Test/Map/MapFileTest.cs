using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EOLib.IO.Map;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using NUnit.Framework;

namespace EOLib.IO.Test.Map
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class MapFileTest
    {
        private IMapFile _mapFile;
        private IMapStringEncoderService ses;
        private INumberEncoderService nes;

        private IMapFileSerializer _serializer;

        [SetUp]
        public void SetUp()
        {
            ses = new MapStringEncoderService();
            nes = new NumberEncoderService();

            _serializer = new MapFileSerializer(
                new MapPropertiesSerializer(nes, ses),
                new NPCSpawnMapEntitySerializer(nes),
                new ChestSpawnMapEntitySerializer(nes),
                new WarpMapEntitySerializer(nes),
                new SignMapEntitySerializer(nes, ses),
                new UnknownMapEntitySerializer(nes),
                nes);
        }

        [Test]
        public void MapFile_Properties_HasExpectedInitialParameters()
        {
            _mapFile = new MapFile().WithMapID(123);

            Assert.That(_mapFile.Properties.MapID, Is.EqualTo(123));
            Assert.That(_mapFile.NPCSpawns, Is.Not.Null);
            Assert.That(_mapFile.Unknowns, Is.Not.Null);
            Assert.That(_mapFile.Chests, Is.Not.Null);
            Assert.That(_mapFile.Tiles, Is.Not.Null);
            Assert.That(_mapFile.Warps, Is.Not.Null);
            Assert.That(_mapFile.GFX, Is.Not.Null);
            Assert.That(_mapFile.Signs, Is.Not.Null);
        }

        [Test]
        public void MapFile_DeserializeFromByteArray_HasCorrectFileSizeInMapProperties()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.None);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.That(_mapFile.Properties.FileSize, Is.EqualTo(mapData.Length));
        }

        [Test]
        public void MapFile_DeserializeFromByteArray_NoTileSpecIsTimedSpikes_FlagIsNotSetInProperties()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.AmbientSource);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.That(_mapFile.Properties.HasTimedSpikes, Is.False);
        }

        [Test]
        public void MapFile_DeserializeFromByteArray_AnyTileSpecIsTimedSpikes_FlagIsSetInProperties()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.SpikesTimed);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.That(_mapFile.Properties.HasTimedSpikes, Is.True);
        }

        [Test]
        public void MapFile_SerializeToByteArray_HasCorrectFormat()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(2).WithHeight(2), TileSpec.Arena, 432);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            var actualData = _serializer.SerializeToByteArray(_mapFile, rewriteChecksum: false);

            Assert.That(mapData, Is.EqualTo(actualData));
        }

        [Test]
        public void MapFile_Width1Height1_HasExpectedGFXAndTiles()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.Board5, 999);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.That(_mapFile.Tiles[1, 1], Is.EqualTo(TileSpec.Board5));
            foreach (var kvp in _mapFile.GFX)
                Assert.That(kvp.Value[1, 1], Is.EqualTo(999));
        }

        [Test]
        public void MapFile_StoresEmptyWarpRows()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.BankVault, 1234);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.That(_mapFile.EmptyWarpRows, Has.Count.EqualTo(1));
        }

        [Test]
        public void MapFile_StoresEmptyTileRows()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.VultTypo, 4321);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.That(_mapFile.EmptyTileRows, Has.Count.EqualTo(1));
        }

        private byte[] CreateDataForMap(IMapFileProperties mapFileProperties, TileSpec spec, int gfx = 1)
        {
            var ret = new List<byte>();

            var serializer = new MapPropertiesSerializer(nes, ses);

            ret.AddRange(serializer.SerializeToByteArray(mapFileProperties));
            ret.AddRange(nes.EncodeNumber(0, 1)); //npc spawns
            ret.AddRange(nes.EncodeNumber(0, 1)); //unknowns
            ret.AddRange(nes.EncodeNumber(0, 1)); //chest spawns

            //tiles
            ret.AddRange(nes.EncodeNumber(2, 1)); //count (rows)
            ret.AddRange(nes.EncodeNumber(1, 1)); //y
            ret.AddRange(nes.EncodeNumber(1, 1)); //count (cols)
            ret.AddRange(nes.EncodeNumber(1, 1)); //x
            ret.AddRange(nes.EncodeNumber((int)spec, 1)); //tilespec
            ret.AddRange(nes.EncodeNumber(0, 1)); //y
            ret.AddRange(nes.EncodeNumber(0, 1)); //count (cols) (empty row)

            //warps (empty row)
            ret.AddRange(nes.EncodeNumber(1, 1)); //count
            ret.AddRange(nes.EncodeNumber(1, 1)); //y
            ret.AddRange(nes.EncodeNumber(0, 1)); //count

            //gfx
            foreach (var layer in (MapLayer[])Enum.GetValues(typeof(MapLayer)))
            {
                ret.AddRange(nes.EncodeNumber(1, 1)); //count
                ret.AddRange(nes.EncodeNumber(1, 1)); //y
                ret.AddRange(nes.EncodeNumber(1, 1)); //count
                ret.AddRange(nes.EncodeNumber(1, 1)); //x
                ret.AddRange(nes.EncodeNumber(gfx, 2)); //gfx value
            }

            ret.AddRange(nes.EncodeNumber(0, 1)); //signs

            return ret.ToArray();
        }
    }
}
