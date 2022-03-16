using EOLib.IO.Pub;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ESFFileTest
    {
        [Test]
        public void HasCorrectFileType()
        {
            Assert.That(new ESFFile().FileType, Is.EqualTo("ESF"));
        }

        [Test]
        public void SerializeToByteArray_ReturnsExpectedBytes()
        {
            var expectedBytes = MakeESFFile(55565554,
                new ESFRecord().WithID(1).WithNames(new List<string> { "TestFixture", "Shout" }),
                new ESFRecord().WithID(2).WithNames(new List<string> { "Test2", "Shout" }),
                new ESFRecord().WithID(3).WithNames(new List<string> { "Test3", "Shout" }),
                new ESFRecord().WithID(4).WithNames(new List<string> { "Test4", "Shout" }),
                new ESFRecord().WithID(5).WithNames(new List<string> { "Test5", "Shout" }),
                new ESFRecord().WithID(6).WithNames(new List<string> { "Test6", "Shout" }),
                new ESFRecord().WithID(7).WithNames(new List<string> { "Test7", "Shout" }),
                new ESFRecord().WithID(8).WithNames(new List<string> { "Test8", "Shout" }),
                new ESFRecord().WithID(9).WithNames(new List<string> { "eof", "eof" }));

            var serializer = CreateFileSerializer();
            var file = serializer.DeserializeFromByteArray(expectedBytes, () => new ESFFile());

            var actualBytes = serializer.SerializeToByteArray(file, rewriteChecksum: false);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void DeserializeFromByteArray_HasExpectedIDAndNames()
        {
            var records = new[]
            {
                new ESFRecord().WithID(1).WithNames(new List<string> { "TestFixture", "Shout" }),
                new ESFRecord().WithID(2).WithNames(new List<string> { "Test2", "Shout" }),
                new ESFRecord().WithID(3).WithNames(new List<string> { "Test3", "Shout" }),
                new ESFRecord().WithID(4).WithNames(new List<string> { "Test4", "Shout" }),
                new ESFRecord().WithID(5).WithNames(new List<string> { "Test5", "Shout" }),
                new ESFRecord().WithID(6).WithNames(new List<string> { "Test6", "Shout" }),
                new ESFRecord().WithID(7).WithNames(new List<string> { "Test7", "Shout" }),
                new ESFRecord().WithID(8).WithNames(new List<string> { "Test8", "Shout" }),
                new ESFRecord().WithID(9).WithNames(new List<string> { "eof", "eof" })
            };
            var bytes = MakeESFFile(55565554, records);

            var serializer = CreateFileSerializer();
            var file = serializer.DeserializeFromByteArray(bytes, () => new ESFFile());

            CollectionAssert.AreEqual(records.Select(x => new { x.ID, x.Name }).ToList(),
                                      file.Select(x => new { x.ID, x.Name }).ToList());
        }

        private byte[] MakeESFFile(int checksum, params IPubRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("ESF"));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
            bytes.AddRange(numberEncoderService.EncodeNumber(records.Length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);

            var recordSerializer = new PubRecordSerializer(numberEncoderService);
            foreach (var record in records)
                bytes.AddRange(recordSerializer.SerializeToByteArray(record));

            return bytes.ToArray();
        }

        private static IPubFileSerializer CreateFileSerializer()
        {
            return new PubFileSerializer(new NumberEncoderService(), new PubRecordSerializer(new NumberEncoderService()));
        }
    }
}
