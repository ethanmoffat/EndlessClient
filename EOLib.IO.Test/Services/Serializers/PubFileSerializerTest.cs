using EOLib.IO.Pub;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace EOLib.IO.Test.Services.Serializers
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class PubFileSerializerTest
    {
        [Test]
        public void EIFFile_DeserializeFromByteArray_WrongLength_Throws()
        {
            const int ExpectedChecksum = 1234567890;
            const int ExpectedLength = 4;

            var records = new[]
            {
                new EIFRecord().WithID(1).WithNames(new List<string> { "Rec_1" }),
                new EIFRecord().WithID(2).WithNames(new List<string> { "Rec_2" }),
                new EIFRecord().WithID(3).WithNames(new List<string> { "Rec_3" }),
                new EIFRecord().WithID(4).WithNames(new List<string> { "Rec_4" }),
            };

            var pubBytesLong = MakePubFileBytes("EIF", ExpectedChecksum, ExpectedLength + 1, records);
            var pubBytesShort = MakePubFileBytes("EIF", ExpectedChecksum, ExpectedLength - 1, records);

            Assert.That(() => CreateSerializer().DeserializeFromByteArray(pubBytesLong, () => new EIFFile()), Throws.InstanceOf<IOException>());
            Assert.That(() => CreateSerializer().DeserializeFromByteArray(pubBytesShort, () => new EIFFile()), Throws.InstanceOf<IOException>());
        }

        [Test]
        public void ENFFile_DeserializeFromByteArray_WrongLength_Throws()
        {
            const int ExpectedChecksum = 1234567890;
            const int ExpectedLength = 4;

            var records = new[]
            {
                new ENFRecord().WithID(1).WithNames(new List<string> { "Rec_1" }),
                new ENFRecord().WithID(2).WithNames(new List<string> { "Rec_2" }),
                new ENFRecord().WithID(3).WithNames(new List<string> { "Rec_3" }),
                new ENFRecord().WithID(4).WithNames(new List<string> { "Rec_4" }),
            };

            var pubBytesLong = MakePubFileBytes("ENF", ExpectedChecksum, ExpectedLength + 1, records);
            var pubBytesShort = MakePubFileBytes("ENF", ExpectedChecksum, ExpectedLength - 1, records);

            Assert.That(() => CreateSerializer().DeserializeFromByteArray(pubBytesLong, () => new ENFFile()), Throws.InstanceOf<IOException>());
            Assert.That(() => CreateSerializer().DeserializeFromByteArray(pubBytesShort, () => new ENFFile()), Throws.InstanceOf<IOException>());
        }

        [Test]
        public void ESFFile_DeserializeFromByteArray_WrongLength_Throws()
        {
            const int ExpectedChecksum = 1234567890;
            const int ExpectedLength = 4;

            var records = new[]
            {
                new ESFRecord().WithID(1).WithNames(new List<string> { "Rec_1", "1_ceR" }),
                new ESFRecord().WithID(2).WithNames(new List<string> { "Rec_2", "2_ceR" }),
                new ESFRecord().WithID(3).WithNames(new List<string> { "Rec_3", "3_ceR" }),
                new ESFRecord().WithID(4).WithNames(new List<string> { "Rec_4", "4_ceR" }),
            };

            var pubBytesLong = MakePubFileBytes("ESF", ExpectedChecksum, ExpectedLength + 1, records);
            var pubBytesShort = MakePubFileBytes("ESF", ExpectedChecksum, ExpectedLength - 1, records);

            Assert.That(() => CreateSerializer().DeserializeFromByteArray(pubBytesLong, () => new ESFFile()), Throws.InstanceOf<IOException>());
            Assert.That(() => CreateSerializer().DeserializeFromByteArray(pubBytesShort, () => new ESFFile()), Throws.InstanceOf<IOException>());
        }

        [Test]
        public void ECFFile_DeserializeFromByteArray_WrongLength_Throws()
        {
            const int ExpectedChecksum = 1234567890;
            const int ExpectedLength = 4;

            var records = new[]
            {
                new ECFRecord().WithID(1).WithNames(new List<string> { "Rec_1" }),
                new ECFRecord().WithID(2).WithNames(new List<string> { "Rec_2" }),
                new ECFRecord().WithID(3).WithNames(new List<string> { "Rec_3" }),
                new ECFRecord().WithID(4).WithNames(new List<string> { "Rec_4" }),
            };

            var pubBytesLong = MakePubFileBytes("ECF", ExpectedChecksum, ExpectedLength + 1, records);
            var pubBytesShort = MakePubFileBytes("ECF", ExpectedChecksum, ExpectedLength - 1, records);

            Assert.That(() => CreateSerializer().DeserializeFromByteArray(pubBytesLong, () => new ECFFile()), Throws.InstanceOf<IOException>());
            Assert.That(() => CreateSerializer().DeserializeFromByteArray(pubBytesShort, () => new ECFFile()), Throws.InstanceOf<IOException>());
        }

        [Test]
        public void EIFFile_DeserializeFromByteArray_HasExpectedHeader()
        {
            const int ExpectedChecksum = 1234567890;
            const int ExpectedLength = 4;

            var records = new[]
            {
                new EIFRecord().WithID(1).WithNames(new List<string> { "Rec_1" }),
                new EIFRecord().WithID(2).WithNames(new List<string> { "Rec_2" }),
                new EIFRecord().WithID(3).WithNames(new List<string> { "Rec_3" }),
                new EIFRecord().WithID(4).WithNames(new List<string> { "Rec_4" }),
            };

            var pubBytes = MakePubFileBytes("EIF", ExpectedChecksum, ExpectedLength, records);
            var file = CreateSerializer().DeserializeFromByteArray(pubBytes, () => new EIFFile());

            Assert.That(file.CheckSum, Is.EqualTo(ExpectedChecksum));
            Assert.That(file.Length, Is.EqualTo(ExpectedLength));
        }

        [Test]
        public void ENFFile_DeserializeFromByteArray_HasExpectedHeader()
        {
            const int ExpectedChecksum = 1234567890;
            const int ExpectedLength = 4;

            var records = new[]
            {
                new ENFRecord().WithID(1).WithNames(new List<string> { "Rec_1" }),
                new ENFRecord().WithID(2).WithNames(new List<string> { "Rec_2" }),
                new ENFRecord().WithID(3).WithNames(new List<string> { "Rec_3" }),
                new ENFRecord().WithID(4).WithNames(new List<string> { "Rec_4" }),
            };

            var pubBytes = MakePubFileBytes("ENF", ExpectedChecksum, ExpectedLength, records);
            var file = CreateSerializer().DeserializeFromByteArray(pubBytes, () => new ENFFile());

            Assert.That(file.CheckSum, Is.EqualTo(ExpectedChecksum));
            Assert.That(file.Length, Is.EqualTo(ExpectedLength));
        }

        [Test]
        public void ESFFile_DeserializeFromByteArray_HasExpectedHeader()
        {
            const int ExpectedChecksum = 1234567890;
            const int ExpectedLength = 4;

            var records = new[]
            {
                new ESFRecord().WithID(1).WithNames(new List<string> { "Rec_1", "1_ceR" }),
                new ESFRecord().WithID(2).WithNames(new List<string> { "Rec_2", "2_ceR" }),
                new ESFRecord().WithID(3).WithNames(new List<string> { "Rec_3", "3_ceR" }),
                new ESFRecord().WithID(4).WithNames(new List<string> { "Rec_4", "4_ceR" }),
            };

            var pubBytes = MakePubFileBytes("ESF", ExpectedChecksum, ExpectedLength, records);
            var file = CreateSerializer().DeserializeFromByteArray(pubBytes, () => new ESFFile());

            Assert.That(file.CheckSum, Is.EqualTo(ExpectedChecksum));
            Assert.That(file.Length, Is.EqualTo(ExpectedLength));
        }

        [Test]
        public void ECFFile_DeserializeFromByteArray_HasExpectedHeader()
        {
            const int ExpectedChecksum = 1234567890;
            const int ExpectedLength = 4;

            var records = new[]
            {
                new ECFRecord().WithID(1).WithNames(new List<string> { "Rec_1" }),
                new ECFRecord().WithID(2).WithNames(new List<string> { "Rec_2" }),
                new ECFRecord().WithID(3).WithNames(new List<string> { "Rec_3" }),
                new ECFRecord().WithID(4).WithNames(new List<string> { "Rec_4" }),
            };

            var pubBytes = MakePubFileBytes("ECF", ExpectedChecksum, ExpectedLength, records);
            var file = CreateSerializer().DeserializeFromByteArray(pubBytes, () => new ECFFile());

            Assert.That(file.CheckSum, Is.EqualTo(ExpectedChecksum));
            Assert.That(file.Length, Is.EqualTo(ExpectedLength));
        }

        private byte[] MakePubFileBytes(string type, int checksum, int length, params IPubRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();
            var recordSerializer = new PubRecordSerializer(numberEncoderService);

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(type));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
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
