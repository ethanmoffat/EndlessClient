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
    public class EIFRecordTest
    {
        [Test]
        public void EIFRecord_GetGlobalPropertyID_GetsRecordID()
        {
            const int expected = 44;
            var rec = new EIFRecord {ID = expected};

            var actual = rec.Get<int>(PubRecordProperty.GlobalID);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void EIFRecord_GetGlobalPropertyName_GetsRecordName()
        {
            const string expected = "some name";
            var rec = new EIFRecord { Name = expected };

            var actual = rec.Get<string>(PubRecordProperty.GlobalName);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void EIFRecord_GetItemPropertiesComprehensive_NoException()
        {
            var itemProperties = Enum.GetNames(typeof (PubRecordProperty))
                                     .Where(x => x.StartsWith("Item"))
                                     .Select(x => (PubRecordProperty) Enum.Parse(typeof (PubRecordProperty), x))
                                     .ToArray();

            Assert.AreNotEqual(0, itemProperties.Length);

            var record = new EIFRecord();

            foreach (var property in itemProperties)
            {
                var dummy = record.Get<object>(property);
                Assert.IsNotNull(dummy);
            }
        }

        [Test]
        public void EIFRecord_GetNPCProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.NPCAccuracy;

            var record = new EIFRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => record.Get<object>(invalidProperty));
        }

        [Test]
        public void EIFRecord_GetSpellProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.SpellAccuracy;

            var record = new EIFRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => record.Get<object>(invalidProperty));
        }

        [Test]
        public void EIFRecord_GetClassProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.ClassAgi;

            var record = new EIFRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => record.Get<object>(invalidProperty));
        }

        [Test]
        public void EIFRecord_InvalidPropertyReturnType_ThrowsInvalidCastException()
        {
            var rec = new EIFRecord {Name = ""};

            Assert.Throws<InvalidCastException>(() => rec.Get<int>(PubRecordProperty.GlobalName));
        }

        [Test]
        public void EIFRecord_SerializeToByteArray_WritesExpectedFormat()
        {
            var numberEncoderService = new NumberEncoderService();
            var record = CreateRecordWithSomeGoodTestData();

            var actualBytes = record.SerializeToByteArray(numberEncoderService);

            var expectedBytes = GetExpectedBytes(record, numberEncoderService);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void EIFRecord_DeserializeFromByteArray_HasCorrectData()
        {
            var numberEncoderService = new NumberEncoderService();
            var sourceRecord = CreateRecordWithSomeGoodTestData();
            var sourceRecordBytes = GetExpectedBytesWithoutName(sourceRecord, numberEncoderService);

            var record = new EIFRecord { ID = sourceRecord.ID, Name = sourceRecord.Name };
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
        public void EIFRecord_DeserializeFromByteArray_InvalidArrayLength_ThrowsException()
        {
            var record = new EIFRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => record.DeserializeFromByteArray(new byte[] { 1, 2, 3 }, new NumberEncoderService()));
        }

        [Test]
        public void EIFRecord_GunOverride_ConvertsItemSubTypeToRanged()
        {
            var record = new EIFRecord {ID = 365, Name = "Gun", SubType = ItemSubType.Arrows};
            record.DeserializeFromByteArray(Enumerable.Repeat((byte)128, EIFRecord.DATA_SIZE).ToArray(), new NumberEncoderService());

            Assert.AreEqual(ItemSubType.Ranged, record.SubType);
        }

        [Test]
        public void EIFRecord_SharedValueSet1_HaveSameValue()
        {
            var properties = new[] { "ScrollMap", "DollGraphic", "ExpReward", "HairColor", "Effect", "Key" };
            var record = new EIFRecord();

            var value = new Random().Next(100);
            foreach (var prop in properties)
            {
                record.GetType().GetProperty(prop, BindingFlags.Instance | BindingFlags.Public)
                                .SetValue(record, value);

                Assert.AreEqual(value, record.ScrollMap);
                Assert.AreEqual(value, record.DollGraphic);
                Assert.AreEqual(value, record.ExpReward);
                Assert.AreEqual(value, record.HairColor);
                Assert.AreEqual(value, record.Effect);
                Assert.AreEqual(value, record.Key);

                value++;
            }
        }

        [Test]
        public void EIFRecord_SharedValueSet2_HaveSameValue()
        {
            var properties = new[] { "Gender", "ScrollX" };
            var record = new EIFRecord();

            var value = new Random().Next(100);
            foreach (var prop in properties)
            {
                record.GetType().GetProperty(prop, BindingFlags.Instance | BindingFlags.Public)
                                .SetValue(record, (byte)value);

                Assert.AreEqual(value, record.Gender);
                Assert.AreEqual(value, record.ScrollX);

                value++;
            }
        }

        [Test]
        public void EIFRecord_SharedValueSet3_HaveSameValue()
        {
            var properties = new[] { "ScrollY", "DualWieldDollGraphic" };
            var record = new EIFRecord();

            var value = new Random().Next(100);
            foreach (var prop in properties)
            {
                record.GetType().GetProperty(prop, BindingFlags.Instance | BindingFlags.Public)
                                .SetValue(record, (byte)value);

                Assert.AreEqual(value, record.ScrollY);
                Assert.AreEqual(value, record.DualWieldDollGraphic);

                value++;
            }
        }

        private static EIFRecord CreateRecordWithSomeGoodTestData()
        {
            return new EIFRecord
            {
                ID = 1,
                Name = "TestName",
                Graphic = 123,
                Type = ItemType.Bracer,
                SubType = ItemSubType.Ranged,
                Special = ItemSpecial.Unique,
                HP = 456,
                TP = 654,
                MinDam = 33,
                MaxDam = 66,
                Accuracy = 100,
                Evade = 200,
                Armor = 300,
                Str = 40,
                Int = 50,
                Wis = 60,
                Agi = 70,
                Con = 80,
                Cha = 90,
                Light = 3,
                Dark = 6,
                Earth = 9,
                Air = 12,
                Water = 15,
                Fire = 18,
                ScrollMap = 33,
                Gender = 44,
                ScrollY = 55,
                LevelReq = 66,
                ClassReq = 77,
                StrReq = 88,
                IntReq = 99,
                WisReq = 30,
                AgiReq = 20,
                ConReq = 10,
                ChaReq = 5,
                Weight = 200,
                Size = ItemSize.Size2x3
            };
        }

        private static byte[] GetExpectedBytes(EIFRecord rec, INumberEncoderService nes)
        {
            var ret = new List<byte>();

            ret.AddRange(nes.EncodeNumber(rec.Name.Length, 1));
            ret.AddRange(Encoding.ASCII.GetBytes(rec.Name));
            ret.AddRange(GetExpectedBytesWithoutName(rec, nes));

            return ret.ToArray();
        }

        private static byte[] GetExpectedBytesWithoutName(EIFRecord rec, INumberEncoderService nes)
        {
            var ret = new List<byte>();

            ret.AddRange(nes.EncodeNumber(rec.Graphic, 2));
            ret.AddRange(nes.EncodeNumber((byte)rec.Type, 1));
            ret.AddRange(nes.EncodeNumber((byte)rec.SubType, 1));
            ret.AddRange(nes.EncodeNumber((byte)rec.Special, 1));
            ret.AddRange(nes.EncodeNumber(rec.HP, 2));
            ret.AddRange(nes.EncodeNumber(rec.TP, 2));
            ret.AddRange(nes.EncodeNumber(rec.MinDam, 2));
            ret.AddRange(nes.EncodeNumber(rec.MaxDam, 2));
            ret.AddRange(nes.EncodeNumber(rec.Accuracy, 2));
            ret.AddRange(nes.EncodeNumber(rec.Evade, 2));
            ret.AddRange(nes.EncodeNumber(rec.Armor, 2));
            ret.AddRange(Enumerable.Repeat((byte)254, 1));
            ret.AddRange(nes.EncodeNumber(rec.Str, 1));
            ret.AddRange(nes.EncodeNumber(rec.Int, 1));
            ret.AddRange(nes.EncodeNumber(rec.Wis, 1));
            ret.AddRange(nes.EncodeNumber(rec.Agi, 1));
            ret.AddRange(nes.EncodeNumber(rec.Con, 1));
            ret.AddRange(nes.EncodeNumber(rec.Cha, 1));
            ret.AddRange(nes.EncodeNumber(rec.Light, 1));
            ret.AddRange(nes.EncodeNumber(rec.Dark, 1));
            ret.AddRange(nes.EncodeNumber(rec.Earth, 1));
            ret.AddRange(nes.EncodeNumber(rec.Air, 1));
            ret.AddRange(nes.EncodeNumber(rec.Water, 1));
            ret.AddRange(nes.EncodeNumber(rec.Fire, 1));
            ret.AddRange(nes.EncodeNumber(rec.ScrollMap, 3));
            ret.AddRange(nes.EncodeNumber(rec.ScrollX, 1));
            ret.AddRange(nes.EncodeNumber(rec.ScrollY, 1));
            ret.AddRange(nes.EncodeNumber(rec.LevelReq, 2));
            ret.AddRange(nes.EncodeNumber(rec.ClassReq, 2));
            ret.AddRange(nes.EncodeNumber(rec.StrReq, 2));
            ret.AddRange(nes.EncodeNumber(rec.IntReq, 2));
            ret.AddRange(nes.EncodeNumber(rec.WisReq, 2));
            ret.AddRange(nes.EncodeNumber(rec.AgiReq, 2));
            ret.AddRange(nes.EncodeNumber(rec.ConReq, 2));
            ret.AddRange(nes.EncodeNumber(rec.ChaReq, 2));
            ret.AddRange(Enumerable.Repeat((byte)254, 2));
            ret.AddRange(nes.EncodeNumber(rec.Weight, 1));
            ret.AddRange(Enumerable.Repeat((byte)254, 1));
            ret.AddRange(nes.EncodeNumber((byte)rec.Size, 1));

            return ret.ToArray();
        }
    }
}
