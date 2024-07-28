using EOLib.IO.Pub;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace EOLib.IO.Test.Services.Serializers
{
    [TestFixture]
    public class PubFileSerializerTest_EIFImpl : PubFileSerializerTest<EIFFile, EIFRecord> { }

    [TestFixture]
    public class PubFileSerializerTest_ENFImpl : PubFileSerializerTest<ENFFile, ENFRecord> { }

    [TestFixture]
    public class PubFileSerializerTest_ESFImpl : PubFileSerializerTest<ESFFile, ESFRecord> { }

    [TestFixture]
    public class PubFileSerializerTest_ECFImpl : PubFileSerializerTest<ECFFile, ECFRecord> { }

    [ExcludeFromCodeCoverage]
    public abstract class PubFileSerializerTest<T, U>
        where T : IPubFile<U>, new()
        where U : class, IPubRecord, new()
    {
        [Test]
        public void DeserializeFromByteArray_WrongLength_Throws()
        {
            const int ExpectedChecksum1 = 12345;
            const int ExpectedChecksum2 = 6789;
            const int ExpectedLength = 4;

            var expectedChecksum = new List<int> { ExpectedChecksum1, ExpectedChecksum2 };

            var records = new[]
            {
                new U().WithID(1).WithName("Rec_1"),
                new U().WithID(2).WithName("Rec_2"),
                new U().WithID(3).WithName("Rec_3"),
                new U().WithID(4).WithName("Rec_4"),
            };

            var pubBytesShort = MakePubFileBytes(expectedChecksum, ExpectedLength - 1, records);
            Assert.That(() => CreateSerializer().DeserializeFromByteArray(1, pubBytesShort, () => new T()), Throws.InstanceOf<IOException>());
        }

        [Test]
        public void DeserializeFromByteArray_HasExpectedHeader()
        {
            const int ExpectedChecksum1 = 12345;
            const int ExpectedChecksum2 = 6789;
            const int ExpectedLength = 4;

            var expectedChecksum = new List<int> { ExpectedChecksum1, ExpectedChecksum2 };

            var records = new[]
            {
                new U().WithID(1).WithName("Rec_1"),
                new U().WithID(2).WithName("Rec_2"),
                new U().WithID(3).WithName("Rec_3"),
                new U().WithID(4).WithName("Rec_4"),
            };

            var pubBytes = MakePubFileBytes(expectedChecksum, ExpectedLength, records);
            var file = CreateSerializer().DeserializeFromByteArray(1, pubBytes, () => new T());

            Assert.That(file.CheckSum, Is.EqualTo(expectedChecksum));
            Assert.That(file.Length, Is.EqualTo(ExpectedLength));
        }

        [Test]
        public void SerializeToByteArray_ReturnsExpectedBytes()
        {
            var expectedBytes = MakePubFileBytes(new List<int> { 5556, 5554 },
                9,
                new U().WithID(1).WithName("TestFixture"),
                new U().WithID(2).WithName("Test2"),
                new U().WithID(3).WithName("Test3"),
                new U().WithID(4).WithName("Test4"),
                new U().WithID(5).WithName("Test5"),
                new U().WithID(6).WithName("Test6"),
                new U().WithID(7).WithName("Test7"),
                new U().WithID(8).WithName("Test8"),
                new U().WithID(9).WithName("eof"));

            var serializer = CreateSerializer();
            var file = serializer.DeserializeFromByteArray(1, expectedBytes, () => new T());

            var actualBytes = serializer.SerializeToByteArray(file, rewriteChecksum: false);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void DeserializeFromByteArray_HasExpectedIDAndNames()
        {
            var records = new[]
            {
                new U().WithID(1).WithName("TestFixture"),
                new U().WithID(2).WithName("Test2"),
                new U().WithID(3).WithName("Test3"),
                new U().WithID(4).WithName("Test4"),
                new U().WithID(5).WithName("Test5"),
                new U().WithID(6).WithName("Test6"),
                new U().WithID(7).WithName("Test7"),
                new U().WithID(8).WithName("Test8"),
                new U().WithID(9).WithName("eof")
            };
            var bytes = MakePubFileBytes(new List<int> { 5556, 5554 }, 9, records);

            var serializer = CreateSerializer();
            var file = serializer.DeserializeFromByteArray(1, bytes, () => new T());

            CollectionAssert.AreEqual(records.Select(x => new { x.ID, x.Name }).ToList(),
                                      file.Select(x => new { x.ID, x.Name }).ToList());
        }

        private byte[] MakePubFileBytes(List<int> checksum, int length, params IPubRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();
            var recordSerializer = new PubRecordSerializer(numberEncoderService);

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(new T().FileType));
            bytes.AddRange(checksum.SelectMany(x => numberEncoderService.EncodeNumber(x, 2)));
            bytes.AddRange(numberEncoderService.EncodeNumber(length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);
            foreach (var record in records)
                bytes.AddRange(recordSerializer.SerializeToByteArray(record));

            return bytes.ToArray();
        }

        private static IPubFileSerializer CreateSerializer()
        {
            return new PubFileSerializer(new NumberEncoderService(), new PubRecordSerializer(new NumberEncoderService()));
        }
    }
}