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
    public class ENFRecordTest
    {
        [TestMethod]
        public void ENFRecord_GetGlobalPropertyID_GetsRecordID()
        {
            const int expected = 44;
            var rec = new ENFRecord { ID = expected };

            var actual = rec.Get<int>(PubRecordProperty.GlobalID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ENFRecord_GetGlobalPropertyName_GetsRecordName()
        {
            const string expected = "some name";
            var rec = new ENFRecord { Name = expected };

            var actual = rec.Get<string>(PubRecordProperty.GlobalName);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
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

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ENFRecord_GetItemProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.ItemSubType;

            var record = new ENFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ENFRecord_GetSpellProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.SpellAccuracy;

            var record = new ENFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ENFRecord_GetClassProperty_ThrowsArgumentOutOfRangeException()
        {
            const PubRecordProperty invalidProperty = PubRecordProperty.ClassAgi;

            var record = new ENFRecord();

            record.Get<object>(invalidProperty);
        }

        [TestMethod, ExpectedException(typeof(InvalidCastException))]
        public void ENFRecord_InvalidPropertyReturnType_ThrowsInvalidCastException()
        {
            var rec = new ENFRecord { Name = "" };

            rec.Get<int>(PubRecordProperty.GlobalName);
        }
    }
}
