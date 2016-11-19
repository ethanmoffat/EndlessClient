// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test.Pub
{
    [TestClass, ExcludeFromCodeCoverage]
    public class ECFRecordTest
    {
        [TestMethod]
        public void ECFRecord_GetGlobalPropertyID_GetsRecordID()
        {
            const int expected = 44;
            var rec = new ECFRecord { ID = expected };

            var actual = rec.Get<int>(PubRecordProperty.GlobalID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ECFRecord_GetGlobalPropertyName_GetsRecordName()
        {
            const string expected = "some name";
            var rec = new ECFRecord { Name = expected };

            var actual = rec.Get<string>(PubRecordProperty.GlobalName);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ECFRecord_GetClassPropertiesComprehensive_NoException()
        {
            var classProperties = Enum.GetNames(typeof(PubRecordProperty))
                                      .Where(x => x.StartsWith("Class"))
                                      .Select(x => (PubRecordProperty)Enum.Parse(typeof(PubRecordProperty), x))
                                      .ToArray();

            Assert.AreNotEqual(0, classProperties.Length);

            var record = new ECFRecord();

            foreach (var property in classProperties)
            {
                var dummy = record.Get<object>(property);
                Assert.IsNotNull(dummy);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ECFRecord_GetItemProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.ItemSubType;

            var record = new ECFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ECFRecord_GetSpellProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.SpellAccuracy;

            var record = new ECFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ECFRecord_GetNPCProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.NPCAccuracy;

            var record = new ECFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(InvalidCastException))]
        public void ECFRecord_InvalidPropertyReturnType_ThrowsInvalidCastException()
        {
            var rec = new ECFRecord { Name = "" };

            rec.Get<int>(PubRecordProperty.GlobalName);
        }

        [TestMethod]
        public void ECFRecord_SerializeToByteArray_WritesExpectedFormat()
        {
            var numberEncoderService = new NumberEncoderService();
            var record = CreateRecordWithSomeGoodTestData();

            var actualBytes = record.SerializeToByteArray(numberEncoderService);

            var expectedBytes = GetExpectedBytes(record, numberEncoderService);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void ECFRecord_DeserializeFromByteArray_HasCorrectData()
        {
            var numberEncoderService = new NumberEncoderService();
            var sourceRecord = CreateRecordWithSomeGoodTestData();
            var sourceRecordBytes = GetExpectedBytesWithoutName(sourceRecord, numberEncoderService);

            var record = new ECFRecord { ID = sourceRecord.ID, Name = sourceRecord.Name };
            record.DeserializeFromByteArray(sourceRecordBytes, numberEncoderService);

            var properties = record.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Assert.IsTrue(properties.Length > 0);

            foreach (var property in properties)
            {
                var expectedValue = property.GetValue(sourceRecord);
                var actualValue = property.GetValue(record);

                Assert.AreEqual(expectedValue, actualValue, "Property: {0}", property.Name);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ECFRecord_DeserializeFromByteArray_InvalidArrayLength_ThrowsException()
        {
            var record = new ECFRecord();

            record.DeserializeFromByteArray(new byte[] { 1, 2, 3 }, new NumberEncoderService());
        }

        private static ECFRecord CreateRecordWithSomeGoodTestData()
        {
            return new ECFRecord
            {
                ID = 1,
                Name = "TestName",

                Base = 33,
                Type = 99,
                
                Str = 10,
                Int = 20,
                Wis = 30,
                Agi = 200,
                Con = 190,
                Cha = 180
            };
        }

        private static byte[] GetExpectedBytes(ECFRecord rec, INumberEncoderService nes)
        {
            var ret = new List<byte>();

            ret.AddRange(nes.EncodeNumber(rec.Name.Length, 1));
            ret.AddRange(Encoding.ASCII.GetBytes(rec.Name));
            ret.AddRange(GetExpectedBytesWithoutName(rec, nes));

            return ret.ToArray();
        }

        private static byte[] GetExpectedBytesWithoutName(ECFRecord rec, INumberEncoderService nes)
        {
            var ret = new List<byte>();

            ret.AddRange(nes.EncodeNumber(rec.Base, 1));
            ret.AddRange(nes.EncodeNumber(rec.Type, 1));
            ret.AddRange(nes.EncodeNumber(rec.Str, 2));
            ret.AddRange(nes.EncodeNumber(rec.Int, 2));
            ret.AddRange(nes.EncodeNumber(rec.Wis, 2));
            ret.AddRange(nes.EncodeNumber(rec.Agi, 2));
            ret.AddRange(nes.EncodeNumber(rec.Con, 2));
            ret.AddRange(nes.EncodeNumber(rec.Cha, 2));

            return ret.ToArray();
        }
    }
}
