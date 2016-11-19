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
    public class ESFRecordTest
    {
        [TestMethod]
        public void ESFRecord_GetGlobalPropertyID_GetsRecordID()
        {
            const int expected = 44;
            var rec = new ESFRecord { ID = expected };

            var actual = rec.Get<int>(PubRecordProperty.GlobalID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ESFRecord_GetGlobalPropertyName_GetsRecordName()
        {
            const string expected = "some name";
            var rec = new ESFRecord { Name = expected };

            var actual = rec.Get<string>(PubRecordProperty.GlobalName);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ESFRecord_GetSpellPropertiesComprehensive_NoException()
        {
            var spellProperties = Enum.GetNames(typeof(PubRecordProperty))
                                      .Where(x => x.StartsWith("Spell"))
                                      .Select(x => (PubRecordProperty)Enum.Parse(typeof(PubRecordProperty), x))
                                      .ToArray();

            Assert.AreNotEqual(0, spellProperties.Length);

            var record = new ESFRecord {Shout = ""};

            foreach (var property in spellProperties)
            {
                var dummy = record.Get<object>(property);
                Assert.IsNotNull(dummy);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ESFRecord_GetItemProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.ItemSubType;

            var record = new ESFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ESFRecord_GetNPCProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.NPCAccuracy;

            var record = new ESFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ESFRecord_GetClassProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.ClassAgi;

            var record = new ESFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(InvalidCastException))]
        public void ESFRecord_InvalidPropertyReturnType_ThrowsInvalidCastException()
        {
            var rec = new ESFRecord { Name = "" };

            rec.Get<int>(PubRecordProperty.GlobalName);
        }

        [TestMethod]
        public void ESFRecord_SerializeToByteArray_WritesExpectedFormat()
        {
            var numberEncoderService = new NumberEncoderService();
            var record = CreateRecordWithSomeGoodTestData();

            var actualBytes = record.SerializeToByteArray(numberEncoderService);

            var expectedBytes = GetExpectedBytes(record, numberEncoderService);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void ESFRecord_DeserializeFromByteArray_HasCorrectData()
        {
            var numberEncoderService = new NumberEncoderService();
            var sourceRecord = CreateRecordWithSomeGoodTestData();
            var sourceRecordBytes = GetExpectedBytesWithoutNames(sourceRecord, numberEncoderService);

            var record = new ESFRecord { ID = sourceRecord.ID, Name = sourceRecord.Name, Shout = sourceRecord.Shout };
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
        public void ESFRecord_DeserializeFromByteArray_InvalidArrayLength_ThrowsException()
        {
            var record = new ESFRecord();

            record.DeserializeFromByteArray(new byte[] { 1, 2, 3 }, new NumberEncoderService());
        }

        private static ESFRecord CreateRecordWithSomeGoodTestData()
        {
            return new ESFRecord
            {
                ID = 1,
                Name = "TestName",
                Shout = "TestShout",
                Icon = 321,
                Graphic = 123,
                TP = 400,
                SP = 900,

                CastTime = 12,

                Type = SpellType.Bard,
                TargetRestrict = SpellTargetRestrict.Opponent,
                Target = SpellTarget.Unknown1,

                MinDam = 3212,
                MaxDam = 16543,
                Accuracy = 222,
                HP = 777
            };
        }

        private static byte[] GetExpectedBytes(ESFRecord rec, INumberEncoderService nes)
        {
            var ret = new List<byte>();

            ret.AddRange(nes.EncodeNumber(rec.Name.Length, 1));
            ret.AddRange(nes.EncodeNumber(rec.Shout.Length, 1));
            ret.AddRange(Encoding.ASCII.GetBytes(rec.Name));
            ret.AddRange(Encoding.ASCII.GetBytes(rec.Shout));
            ret.AddRange(GetExpectedBytesWithoutNames(rec, nes));

            return ret.ToArray();
        }

        private static byte[] GetExpectedBytesWithoutNames(ESFRecord rec, INumberEncoderService nes)
        {
            var ret = new List<byte>();

            ret.AddRange(nes.EncodeNumber(rec.Icon, 2));
            ret.AddRange(nes.EncodeNumber(rec.Graphic, 2));
            ret.AddRange(nes.EncodeNumber(rec.TP, 2));
            ret.AddRange(nes.EncodeNumber(rec.SP, 2));
            ret.AddRange(nes.EncodeNumber(rec.CastTime, 1));
            ret.AddRange(Enumerable.Repeat((byte)254, 2));
            ret.AddRange(nes.EncodeNumber((byte)rec.Type, 1));
            ret.AddRange(Enumerable.Repeat((byte)254, 5));
            ret.AddRange(nes.EncodeNumber((byte)rec.TargetRestrict, 1));
            ret.AddRange(nes.EncodeNumber((byte)rec.Target, 1));
            ret.AddRange(Enumerable.Repeat((byte)254, 4));
            ret.AddRange(nes.EncodeNumber(rec.MinDam, 2));
            ret.AddRange(nes.EncodeNumber(rec.MaxDam, 2));
            ret.AddRange(nes.EncodeNumber(rec.Accuracy, 2));
            ret.AddRange(Enumerable.Repeat((byte)254, 5));
            ret.AddRange(nes.EncodeNumber(rec.HP, 2));
            ret.AddRange(Enumerable.Repeat((byte)254, 15));

            return ret.ToArray();
        }
    }
}
