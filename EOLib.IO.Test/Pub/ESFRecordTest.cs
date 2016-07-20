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
    public class ESFRecordTest
    {
        [TestMethod]
        public void ESFRecord_GetGlobalPropertyID_GetsRecordID()
        {
            const int expected = 44;
            var rec = new ESFRecord { ID = expected };

            var actual = rec.Get<int>(PubRecordPropertyType.GlobalID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ESFRecord_GetGlobalPropertyName_GetsRecordName()
        {
            const string expected = "some name";
            var rec = new ESFRecord { Name = expected };

            var actual = rec.Get<string>(PubRecordPropertyType.GlobalName);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ESFRecord_GetSpellPropertiesComprehensive_NoException()
        {
            var spellProperties = Enum.GetNames(typeof(PubRecordPropertyType))
                                      .Where(x => x.StartsWith("Spell"))
                                      .Select(x => (PubRecordPropertyType)Enum.Parse(typeof(PubRecordPropertyType), x))
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
            const PubRecordPropertyType invalidProperty = PubRecordPropertyType.ItemSubType;

            var record = new ESFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ESFRecord_GetNPCProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordPropertyType invalidProperty = PubRecordPropertyType.NPCAccuracy;

            var record = new ESFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ESFRecord_GetClassProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordPropertyType invalidProperty = PubRecordPropertyType.ClassAgi;

            var record = new ESFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(InvalidCastException))]
        public void ESFRecord_InvalidPropertyReturnType_ThrowsInvalidCastException()
        {
            var rec = new ESFRecord { Name = "" };

            rec.Get<int>(PubRecordPropertyType.GlobalName);
        }
    }
}
