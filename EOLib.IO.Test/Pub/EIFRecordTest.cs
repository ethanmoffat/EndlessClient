// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EOLib.IO.Pub;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test.Pub
{
    [TestClass]
    public class EIFRecordTest
    {
        [TestMethod]
        public void EIFRecord_GetGlobalPropertyID_GetsRecordID()
        {
            const int expected = 44;
            var rec = new EIFRecord {ID = expected};

            var actual = rec.Get<int>(PubRecordPropertyType.GlobalID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EIFRecord_GetGlobalPropertyName_GetsRecordName()
        {
            const string expected = "some name";
            var rec = new EIFRecord { Name = expected };

            var actual = rec.Get<string>(PubRecordPropertyType.GlobalName);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EIFRecord_GetItemPropertiesComprehensive_NoException()
        {
            var itemProperties = Enum.GetNames(typeof (PubRecordPropertyType))
                                     .Where(x => x.StartsWith("Item"))
                                     .Select(x => (PubRecordPropertyType) Enum.Parse(typeof (PubRecordPropertyType), x))
                                     .ToArray();

            Assert.AreNotEqual(0, itemProperties.Length);

            var record = new EIFRecord();

            foreach (var property in itemProperties)
            {
                var dummy = record.Get<object>(property);
                Assert.IsNotNull(dummy);
            }
        }

        [TestMethod, ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void EIFRecord_GetNPCProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordPropertyType invalidProperty = PubRecordPropertyType.NPCAccuracy;

            var record = new EIFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EIFRecord_GetSpellProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordPropertyType invalidProperty = PubRecordPropertyType.SpellAccuracy;

            var record = new EIFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EIFRecord_GetClassProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordPropertyType invalidProperty = PubRecordPropertyType.ClassAgi;

            var record = new EIFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof (InvalidCastException))]
        public void EIFRecord_InvalidPropertyReturnType_ThrowsInvalidCastException()
        {
            var rec = new EIFRecord {Name = ""};

            rec.Get<int>(PubRecordPropertyType.GlobalName);
        }
    }
}
