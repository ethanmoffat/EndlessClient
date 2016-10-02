// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EOLib.IO.Map;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test.Map
{
    [TestClass, ExcludeFromCodeCoverage]
    public class MapFileTest
    {
        private IMapFile _mapFile;
        private IMapStringEncoderService ses;
        private INumberEncoderService nes;

        private ISerializer<IMapFile> _serializer;

        [TestInitialize]
        public void TestInitialize()
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

        [TestMethod]
        public void MapFile_Properties_HasExpectedInitialParameters()
        {
            _mapFile = new MapFile().WithMapID(123);

            Assert.AreEqual(123, _mapFile.Properties.MapID);
            Assert.IsNotNull(_mapFile.NPCSpawns);
            Assert.IsNotNull(_mapFile.Unknowns);
            Assert.IsNotNull(_mapFile.Chests);
            Assert.IsNotNull(_mapFile.Tiles);
            Assert.IsNotNull(_mapFile.Warps);
            Assert.IsNotNull(_mapFile.GFX);
            Assert.IsNotNull(_mapFile.Signs);
        }

        [TestMethod]
        public void MapFile_DeserializeFromByteArray_HasCorrectFileSizeInMapProperties()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.None);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.AreEqual(mapData.Length, _mapFile.Properties.FileSize);
        }

        [TestMethod]
        public void MapFile_DeserializeFromByteArray_NoTileSpecIsTimedSpikes_FlagIsNotSetInProperties()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.AmbientSource);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.IsFalse(_mapFile.Properties.HasTimedSpikes);
        }

        [TestMethod]
        public void MapFile_DeserializeFromByteArray_AnyTileSpecIsTimedSpikes_FlagIsSetInProperties()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.SpikesTimed);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.IsTrue(_mapFile.Properties.HasTimedSpikes);
        }

        [TestMethod]
        public void MapFile_SerializeToByteArray_HasCorrectFormat()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(2).WithHeight(2), TileSpec.Arena, 432);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            var actualData = _serializer.SerializeToByteArray(_mapFile);

            CollectionAssert.AreEqual(mapData, actualData);
        }

        [TestMethod]
        public void MapFile_Width1Height1_HasExpectedGFXAndTiles()
        {
            _mapFile = new MapFile().WithMapID(1);

            var mapData = CreateDataForMap(new MapFileProperties().WithWidth(1).WithHeight(1), TileSpec.Board5, 999);
            _mapFile = _serializer.DeserializeFromByteArray(mapData);

            Assert.AreEqual(TileSpec.Board5, _mapFile.Tiles[1, 1]);
            foreach (var kvp in _mapFile.GFX)
                Assert.AreEqual(999, kvp.Value[1, 1]);
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
            ret.AddRange(nes.EncodeNumber(1, 1)); //count
            ret.AddRange(nes.EncodeNumber(1, 1)); //y
            ret.AddRange(nes.EncodeNumber(1, 1)); //count
            ret.AddRange(nes.EncodeNumber(1, 1)); //x
            ret.AddRange(nes.EncodeNumber((byte)spec, 1)); //tilespec

            //warps
            ret.AddRange(nes.EncodeNumber(0, 1));

            //gfx
            foreach (var layer in (MapLayer[]) Enum.GetValues(typeof(MapLayer)))
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
