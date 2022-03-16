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
    public class EIFFileTest
    {
        [Test]
        public void HasCorrectFileType()
        {
            Assert.That(new EIFFile().FileType, Is.EqualTo("EIF"));
        }

        [Test]
        public void SerializeToByteArray_ReturnsExpectedBytes()
        {
            var expectedBytes = MakeEIFFile(55565554,
                new EIFRecord().WithID(1).WithNames(new List<string> { "TestFixture" }),
                new EIFRecord().WithID(2).WithNames(new List<string> { "Test2" }),
                new EIFRecord().WithID(3).WithNames(new List<string> { "Test3" }),
                new EIFRecord().WithID(4).WithNames(new List<string> { "Test4" }),
                new EIFRecord().WithID(5).WithNames(new List<string> { "Test5" }),
                new EIFRecord().WithID(6).WithNames(new List<string> { "Test6" }),
                new EIFRecord().WithID(7).WithNames(new List<string> { "Test7" }),
                new EIFRecord().WithID(8).WithNames(new List<string> { "Test8" }),
                new EIFRecord().WithID(9).WithNames(new List<string> { "eof" }));

            var serializer = CreateFileSerializer();
            var file = serializer.DeserializeFromByteArray(expectedBytes, () => new EIFFile());

            var actualBytes = serializer.SerializeToByteArray(file, rewriteChecksum: false);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void DeserializeFromByteArray_HasExpectedIDAndNames()
        {
            var records = new[]
            {
                new EIFRecord().WithID(1).WithNames(new List<string> { "TestFixture" }),
                new EIFRecord().WithID(2).WithNames(new List<string> { "Test2" }),
                new EIFRecord().WithID(3).WithNames(new List<string> { "Test3" }),
                new EIFRecord().WithID(4).WithNames(new List<string> { "Test4" }),
                new EIFRecord().WithID(5).WithNames(new List<string> { "Test5" }),
                new EIFRecord().WithID(6).WithNames(new List<string> { "Test6" }),
                new EIFRecord().WithID(7).WithNames(new List<string> { "Test7" }),
                new EIFRecord().WithID(8).WithNames(new List<string> { "Test8" }),
                new EIFRecord().WithID(9).WithNames(new List<string> { "eof" })
            };
            var bytes = MakeEIFFile(55565554, records);

            var serializer = CreateFileSerializer();
            var file = serializer.DeserializeFromByteArray(bytes, () => new EIFFile());

            CollectionAssert.AreEqual(records.Select(x => new { x.ID, x.Name }).ToList(),
                                      file.Select(x => new { x.ID, x.Name }).ToList());
        }

        private byte[] MakeEIFFile(int checksum, params IPubRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("EIF"));
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
