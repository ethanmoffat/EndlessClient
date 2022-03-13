using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using NUnit.Framework;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ENFRecordTest
    {
        [Test]
        public void ENFRecord_GetGlobalPropertyID_GetsRecordID()
        {
            const int expected = 44;
            var rec = new ENFRecord { ID = expected };

            var actual = rec.Get<int>(PubRecordProperty.GlobalID);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ENFRecord_GetGlobalPropertyName_GetsRecordName()
        {
            const string expected = "some name";
            var rec = new ENFRecord { Name = expected };

            var actual = rec.Get<string>(PubRecordProperty.GlobalName);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ENFRecord_GetNPCPropertiesComprehensive_NoException()
        {
            var npcProperties = Enum.GetNames(typeof(PubRecordProperty))
                                    .Where(x => x.StartsWith("NPC"))
                                    .Select(x => (PubRecordProperty)Enum.Parse(typeof(PubRecordProperty), x))
                                    .ToArray();

            Assert.AreNotEqual(0, npcProperties.Length);

            var record = new ENFRecord();

            foreach (var property in npcProperties)
            {
                var dummy = record.Get<object>(property);
                Assert.IsNotNull(dummy);
            }
        }

        [Test]
        public void ENFRecord_GetItemProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.ItemSubType;

            var record = new ENFRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => record.Get<object>(invalidProperty));
        }

        [Test]
        public void ENFRecord_GetSpellProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.SpellAccuracy;

            var record = new ENFRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => record.Get<object>(invalidProperty));
        }

        [Test]
        public void ENFRecord_GetClassProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.ClassAgi;

            var record = new ENFRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => record.Get<object>(invalidProperty));
        }

        [Test]
        public void ENFRecord_InvalidPropertyReturnType_ThrowsInvalidCastException()
        {
            var rec = new ENFRecord { Name = "" };

            Assert.Throws<InvalidCastException>(() => rec.Get<int>(PubRecordProperty.GlobalName));
        }

        [Test]
        public void ENFRecord_SerializeToByteArray_WritesExpectedFormat()
        {
            var numberEncoderService = new NumberEncoderService();
            var record = CreateRecordWithSomeGoodTestData();

            var actualBytes = record.SerializeToByteArray(numberEncoderService);

            var expectedBytes = GetExpectedBytes(record, numberEncoderService);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void ENFRecord_DeserializeFromByteArray_HasCorrectData()
        {
            var numberEncoderService = new NumberEncoderService();
            var sourceRecord = CreateRecordWithSomeGoodTestData();
            var sourceRecordBytes = GetExpectedBytesWithoutName(sourceRecord, numberEncoderService);

            var record = new ENFRecord { ID = sourceRecord.ID, Name = sourceRecord.Name };
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

        [Test]
        public void ENFRecord_DeserializeFromByteArray_InvalidArrayLength_ThrowsException()
        {
            var record = new ENFRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => record.DeserializeFromByteArray(new byte[] { 1, 2, 3 }, new NumberEncoderService()));
        }

        private static ENFRecord CreateRecordWithSomeGoodTestData()
        {
            return new ENFRecord
            {
                ID = 1,
                Name = "TestName",
                Graphic = 123,
                Boss = 321,
                Child = 4321,
                Type = NPCType.Barber,

                VendorID = 1234,

                HP = 123456,
                Exp = 44332,
                MinDam = 16543,
                MaxDam = 16544,

                Accuracy = 31313,
                Evade = 13131,
                Armor = 222
            };
        }

        private static byte[] GetExpectedBytes(ENFRecord rec, INumberEncoderService nes)
        {
            var ret = new List<byte>();

            ret.AddRange(nes.EncodeNumber(rec.Name.Length, 1));
            ret.AddRange(Encoding.ASCII.GetBytes(rec.Name));
            ret.AddRange(GetExpectedBytesWithoutName(rec, nes));

            return ret.ToArray();
        }

        private static byte[] GetExpectedBytesWithoutName(ENFRecord rec, INumberEncoderService nes)
        {
            var ret = new List<byte>();

            ret.AddRange(nes.EncodeNumber(rec.Graphic, 2));
            ret.AddRange(nes.EncodeNumber(rec.UnkByte2, 1));
            ret.AddRange(nes.EncodeNumber(rec.Boss, 2));
            ret.AddRange(nes.EncodeNumber(rec.Child, 2));
            ret.AddRange(nes.EncodeNumber((short)rec.Type, 2));
            ret.AddRange(nes.EncodeNumber(rec.VendorID, 2));
            ret.AddRange(nes.EncodeNumber(rec.HP, 3));
            ret.AddRange(nes.EncodeNumber(rec.UnkShort14, 2));
            ret.AddRange(nes.EncodeNumber(rec.MinDam, 2));
            ret.AddRange(nes.EncodeNumber(rec.MaxDam, 2));
            ret.AddRange(nes.EncodeNumber(rec.Accuracy, 2));
            ret.AddRange(nes.EncodeNumber(rec.Evade, 2));
            ret.AddRange(nes.EncodeNumber(rec.Armor, 2));
            ret.AddRange(nes.EncodeNumber(rec.UnkByte26, 1));
            ret.AddRange(nes.EncodeNumber(rec.UnkShort27, 2));
            ret.AddRange(nes.EncodeNumber(rec.UnkShort29, 2));
            ret.AddRange(nes.EncodeNumber(rec.ElementWeak, 2));
            ret.AddRange(nes.EncodeNumber(rec.ElementWeakPower, 2));
            ret.AddRange(nes.EncodeNumber(rec.UnkByte35, 1));
            ret.AddRange(nes.EncodeNumber(rec.Exp, 3));

            return ret.ToArray();
        }
    }
}
