// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EOLib.IO.Map;
using EOLib.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test
{
    [TestClass]
    public class MapFilePropertiesTest
    {
        private IMapFileProperties _props;

        [TestInitialize]
        public void TestInitialize()
        {
            _props = new MapFileProperties();
        }

        [TestMethod]
        public void MapFileProperties_HasExpectedFileHeader()
        {
            Assert.AreEqual("EMF", _props.FileType);
        }

        [TestMethod]
        public void MapFileProperties_SerializeToByteArray_HasExpectedFormat()
        {
            _props = CreateMapPropertiesWithSomeTestData(_props);

            var expectedBytes = CreateExpectedBytes(_props);
            var actualBytes = _props.SerializeToByteArray(new NumberEncoderService(), new MapStringEncoderService());

            CollectionAssert.AreEquivalent(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void MapFileProperties_DeserializeFromByteArray_HasExpectedValues()
        {
            var expected = CreateMapPropertiesWithSomeTestData(_props);
            var bytes = CreateExpectedBytes(expected);

            _props = _props.DeserializeFromByteArray(bytes, new NumberEncoderService(), new MapStringEncoderService());

            foreach (var property in expected.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var expectedValue = property.GetValue(expected);
                var actualValue = property.GetValue(_props);

                if (expectedValue is ICollection && actualValue is ICollection)
                    CollectionAssert.AreEquivalent((ICollection) expectedValue, (ICollection) actualValue);
                else
                    Assert.AreEqual(expectedValue, actualValue, "Property {0} is not equal!", property.Name);
            }
        }

        [TestMethod]
        public void MapFileProperties_CustomProperties_NotChangedWhenDeserialized()
        {
            var expected = CreateMapPropertiesWithSomeTestData(_props);
            var bytes = CreateExpectedBytes(expected);

            _props = _props.DeserializeFromByteArray(bytes, new NumberEncoderService(), new MapStringEncoderService());

            Assert.AreEqual(new MapFileProperties().MapID, _props.MapID);
            Assert.AreEqual(new MapFileProperties().FileSize, _props.FileSize);
            Assert.AreEqual(new MapFileProperties().HasTimedSpikes, _props.HasTimedSpikes);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void MapFileProperties_DeserializeFromByteArray_ThrowsExceptionWhenIncorrectSize()
        {
            var bytes = new byte[] {1, 2};

            _props.DeserializeFromByteArray(bytes, new NumberEncoderService(), new MapStringEncoderService());
        }

        [TestMethod, ExpectedException(typeof(FormatException))]
        public void MapFileProperties_DeserializeFromByteArray_ThrowsExceptionWhenNotEMF()
        {
            var bytes = Enumerable.Repeat((byte) 254, MapFileProperties.DATA_SIZE).ToArray();

            _props.DeserializeFromByteArray(bytes, new NumberEncoderService(), new MapStringEncoderService());
        }

        private static IMapFileProperties CreateMapPropertiesWithSomeTestData(IMapFileProperties props)
        {
            return props.WithChecksum(new byte[] {1, 2, 3, 4})
                .WithName("Some test name")
                .WithWidth(200)
                .WithHeight(100)
                .WithEffect(MapEffect.Quake)
                .WithMusic(123)
                .WithMusicExtra(98)
                .WithAmbientNoise(4567)
                .WithFillTile(6969)
                .WithRelogX(33)
                .WithRelogY(22)
                .WithUnknown2(100)
                .WithMapAvailable(true)
                .WithScrollAvailable(false)
                .WithPKAvailable(true);
        }

        private static byte[] CreateExpectedBytes(IMapFileProperties props)
        {
            var numberEncoderService = new NumberEncoderService();
            var mapStringEncoderService = new MapStringEncoderService();
            var ret = new List<byte>();

            ret.AddRange(Encoding.ASCII.GetBytes(props.FileType));
            ret.AddRange(props.Checksum);
            
            var fullName = Enumerable.Repeat((byte)0xFF, 24).ToArray();
            var encodedName = mapStringEncoderService.EncodeMapString(props.Name);
            Array.Copy(encodedName, 0, fullName, fullName.Length - encodedName.Length, encodedName.Length);
            ret.AddRange(fullName);

            ret.AddRange(numberEncoderService.EncodeNumber(props.PKAvailable ? 3 : 0, 1));
            ret.AddRange(numberEncoderService.EncodeNumber((byte)props.Effect, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.Music, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.MusicExtra, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.AmbientNoise, 2));
            ret.AddRange(numberEncoderService.EncodeNumber(props.Width, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.Height, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.FillTile, 2));
            ret.AddRange(numberEncoderService.EncodeNumber(props.MapAvailable ? 1 : 0, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.CanScroll ? 1 : 0, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.RelogX, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.RelogY, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.Unknown2, 1));

            return ret.ToArray();
        }
    }
}
