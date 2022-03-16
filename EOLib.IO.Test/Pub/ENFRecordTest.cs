using EOLib.IO.Pub;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ENFRecordTest
    {
        [Test]
        public void ENFRecord_HasAllExpectedProperties()
        {
            var record = new ENFRecord();

            var expectedProperties = ((PubRecordProperty[])Enum.GetValues(typeof(PubRecordProperty)))
                .Where(x => x.HasFlag(PubRecordProperty.NPC))
                .Except(new[] { PubRecordProperty.NPC });

            Assert.That(record.Bag.Count, Is.EqualTo(expectedProperties.Count()));

            foreach (var p in expectedProperties)
                Assert.That(record.Bag, Does.ContainKey(p));
        }

        [Test]
        public void ENFRecord_HasExpectedDataSize()
        {
            const int ExpectedDataSize = 39;
            Assert.That(new ENFRecord().DataSize, Is.EqualTo(ExpectedDataSize));
        }
    }
}
