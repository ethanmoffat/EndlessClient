using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EOLib.IO.Pub;
using NUnit.Framework;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class EIFRecordTest
    {
        [Test]
        public void EIFRecord_HasAllExpectedProperties()
        {
            var record = new EIFRecord();

            var expectedProperties = ((PubRecordProperty[])Enum.GetValues(typeof(PubRecordProperty)))
                .Where(x => x.HasFlag(PubRecordProperty.Item))
                .Except(new[] { PubRecordProperty.Item });

            Assert.That(record.Bag.Count, Is.EqualTo(expectedProperties.Count()));

            foreach (var p in expectedProperties)
                Assert.That(record.Bag, Does.ContainKey(p));
        }

        [Test]
        public void EIFRecord_HasExpectedDataSize()
        {
            const int ExpectedDataSize = 58;
            Assert.That(new EIFRecord().DataSize, Is.EqualTo(ExpectedDataSize));
        }
    }
}
