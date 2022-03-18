using EOLib.IO.Pub;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using NUnit.Framework;
using System.Collections.Generic;

namespace EOLib.IO.Test.Services.Serializers
{
    [TestFixture]
    public class PubRecordSerializerTest_EIFRecordImpl : PubRecordSerializerTest<EIFRecord> { }

    [TestFixture]
    public class PubRecordSerializerTest_ENFRecordImpl : PubRecordSerializerTest<ENFRecord> { }

    [TestFixture]
    public class PubRecordSerializerTest_ESFRecordImpl : PubRecordSerializerTest<ESFRecord> { }

    [TestFixture]
    public class PubRecordSerializerTest_ECFRecordImpl : PubRecordSerializerTest<ECFRecord> { }

    public abstract class PubRecordSerializerTest<T>
        where T : class, IPubRecord, new()
    {
        [Test]
        public void SerializeAndDeserialize_HasExpectedProperties()
        {
            var record = new T();

            var names = new List<string>();
            for (int nameNdx = 0; nameNdx < record.NumberOfNames; nameNdx++)
                names.Add($"name {nameNdx + 1}");
            record = (T)record.WithNames(names);

            int i = 1;
            var expectedValues = new List<(PubRecordProperty Key, int Value)>(record.Bag.Count);
            var offsets = new HashSet<int>();
            foreach (var property in record.Bag.Keys)
            {
                // don't overwrite values for duplicate offsets (unions in records)
                if (offsets.Contains(record.Bag[property].Offset))
                    continue;

                offsets.Add(record.Bag[property].Offset);

                record = (T)record.WithProperty(property, i);
                expectedValues.Add((property, i++));
            }

            var serializer = Create();
            var bytes = serializer.SerializeToByteArray(record);

            var loadedRecord = serializer.DeserializeFromByteArray(bytes, () => new T());

            Assert.That(record, Is.EqualTo(loadedRecord));
        }

        private static IPubRecordSerializer Create()
        {
            return new PubRecordSerializer(new NumberEncoderService());
        }
    }
}
