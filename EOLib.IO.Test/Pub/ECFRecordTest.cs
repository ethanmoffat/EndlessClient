using EOLib.IO.Pub;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ECFRecordTest
    {
        [Test]
        public void ECFRecord_HasAllExpectedProperties()
        {
            var record = new ECFRecord();

            var expectedProperties = ((PubRecordProperty[])Enum.GetValues(typeof(PubRecordProperty)))
                .Where(x => x.HasFlag(PubRecordProperty.Class))
                .Except(new[] { PubRecordProperty.Class });

            Assert.That(record.Bag.Count, Is.EqualTo(expectedProperties.Count()));

            foreach (var p in expectedProperties)
                Assert.That(record.Bag, Does.ContainKey(p));
        }

        [Test]
        public void ECFRecord_HasExpectedDataSize()
        {
            const int ExpectedDataSize = 14;
            Assert.That(new ECFRecord().DataSize, Is.EqualTo(ExpectedDataSize));
        }
    }
}
