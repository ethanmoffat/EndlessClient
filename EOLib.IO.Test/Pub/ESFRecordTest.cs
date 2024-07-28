using EOLib.IO.Pub;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ESFRecordTest
    {
        [Test]
        public void ESFRecord_HasAllExpectedProperties()
        {
            var record = new ESFRecord();

            var expectedProperties = ((PubRecordProperty[])Enum.GetValues(typeof(PubRecordProperty)))
                .Where(x => x.HasFlag(PubRecordProperty.Spell))
                .Except(new[] { PubRecordProperty.Spell });

            Assert.That(record.Bag.Count, Is.EqualTo(expectedProperties.Count()));

            foreach (var p in expectedProperties)
                Assert.That(record.Bag, Does.ContainKey(p));
        }

        [Test]
        public void ESFRecord_HasExpectedDataSize()
        {
            const int ExpectedDataSize = 51;
            Assert.That(new ESFRecord().DataSize, Is.EqualTo(ExpectedDataSize));
        }
    }
}