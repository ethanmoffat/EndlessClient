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
    }
}
