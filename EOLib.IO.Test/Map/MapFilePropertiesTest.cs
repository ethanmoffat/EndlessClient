﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using EOLib.IO.Map;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using NUnit.Framework;

namespace EOLib.IO.Test.Map
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class MapFilePropertiesTest
    {
        private IMapEntitySerializer<IMapFileProperties> _mapPropertiesSerializer;
        private IMapFileProperties _props;

        [SetUp]
        public void SetUp()
        {
            _props = new MapFileProperties();

            _mapPropertiesSerializer = new MapPropertiesSerializer(
                new NumberEncoderService(), new MapStringEncoderService());
        }

        [Test]
        public void MapFileProperties_HasExpectedFileHeader()
        {
            Assert.That(_props.FileType, Is.EqualTo("EMF"));
        }

        [Test]
        public void MapFileProperties_SerializeToByteArray_HasExpectedFormat()
        {
            _props = CreateMapPropertiesWithSomeTestData(_props);

            var expectedBytes = CreateExpectedBytes(_props);
            var actualBytes = _mapPropertiesSerializer.SerializeToByteArray(_props);

            Assert.That(expectedBytes, Is.EqualTo(actualBytes));
        }

        [Test]
        public void MapFileProperties_DeserializeFromByteArray_HasExpectedValues()
        {
            var expected = CreateMapPropertiesWithSomeTestData(_props);
            var bytes = CreateExpectedBytes(expected);

            _props = _mapPropertiesSerializer.DeserializeFromByteArray(bytes);

            foreach (var property in expected.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var expectedValue = property.GetValue(expected);
                var actualValue = property.GetValue(_props);

                if (expectedValue is ICollection && actualValue is ICollection)
                    Assert.That((ICollection)expectedValue, Is.EqualTo(actualValue));
                else
                    Assert.That(actualValue, Is.EqualTo(expectedValue), "Property {0} is not equal", property.Name);
            }
        }

        [Test]
        public void MapFileProperties_CustomProperties_NotChangedWhenDeserialized()
        {
            var expected = CreateMapPropertiesWithSomeTestData(_props);
            var bytes = CreateExpectedBytes(expected);

            _props = _mapPropertiesSerializer.DeserializeFromByteArray(bytes);

            Assert.That(_props.MapID, Is.EqualTo(new MapFileProperties().MapID));
            Assert.That(_props.FileSize, Is.EqualTo(new MapFileProperties().FileSize));
            Assert.That(_props.HasTimedSpikes, Is.EqualTo(new MapFileProperties().HasTimedSpikes));
        }

        [Test]
        public void MapFileProperties_DeserializeFromByteArray_ThrowsExceptionWhenIncorrectSize()
        {
            var bytes = new byte[] { 1, 2 };
            Assert.Throws<ArgumentException>(() => _mapPropertiesSerializer.DeserializeFromByteArray(bytes));
        }

        [Test]
        public void MapFileProperties_DeserializeFromByteArray_ThrowsExceptionWhenNotEMF()
        {
            var bytes = Enumerable.Repeat((byte)254, MapFileProperties.DATA_SIZE).ToArray();
            Assert.Throws<FormatException>(() => _mapPropertiesSerializer.DeserializeFromByteArray(bytes));
        }

        private static IMapFileProperties CreateMapPropertiesWithSomeTestData(IMapFileProperties props)
        {
            return props.WithChecksum(new List<int> { 1, 2 })
                .WithName("Some test name")
                .WithWidth(200)
                .WithHeight(100)
                .WithEffect(MapEffect.Quake1)
                .WithMusic(123)
                .WithControl(MusicControl.InterruptIfDifferentPlayOnce)
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
            ret.AddRange(props.Checksum.SelectMany(x => numberEncoderService.EncodeNumber(x, 2)));

            var fullName = Enumerable.Repeat((byte)0xFF, 24).ToArray();
            var encodedName = mapStringEncoderService.EncodeMapString(props.Name, props.Name.Length);
            Array.Copy(encodedName, 0, fullName, fullName.Length - encodedName.Length, encodedName.Length);
            ret.AddRange(fullName);

            ret.AddRange(numberEncoderService.EncodeNumber(props.PKAvailable ? 3 : 0, 1));
            ret.AddRange(numberEncoderService.EncodeNumber((int)props.Effect, 1));
            ret.AddRange(numberEncoderService.EncodeNumber(props.Music, 1));
            ret.AddRange(numberEncoderService.EncodeNumber((int)props.Control, 1));
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
