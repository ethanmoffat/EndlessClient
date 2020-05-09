using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using NUnit.Framework;

namespace EOLib.IO.Test.Pub
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ECFFileTest
    {
        private IPubFile<ECFRecord> _classFile;

        [SetUp]
        public void SetUp()
        {
            _classFile = new ECFFile();
        }

        [Test]
        public void HasCorrectFileType()
        {
            Assert.AreEqual("ECF", _classFile.FileType);
        }

        [Test]
        public void SerializeToByteArray_ReturnsExpectedBytes()
        {
            var expectedBytes = MakeECFFile(55565554,
                new ECFRecord { ID = 1, Name = "TestFixture" },
                new ECFRecord { ID = 2, Name = "Test2" },
                new ECFRecord { ID = 3, Name = "Test3" },
                new ECFRecord { ID = 4, Name = "Test4" },
                new ECFRecord { ID = 5, Name = "Test5" },
                new ECFRecord { ID = 6, Name = "Test6" },
                new ECFRecord { ID = 7, Name = "Test7" },
                new ECFRecord { ID = 8, Name = "Test8" },
                new ECFRecord { ID = 9, Name = "eof" });

            _classFile.DeserializeFromByteArray(expectedBytes, new NumberEncoderService());

            var actualBytes = _classFile.SerializeToByteArray(new NumberEncoderService());

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void HeaderFormat_IsCorrect()
        {
            var nes = new NumberEncoderService();

            var actualBytes = _classFile.SerializeToByteArray(nes);

            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes(_classFile.FileType), actualBytes.Take(3).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_classFile.CheckSum, 4), actualBytes.Skip(3).Take(4).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_classFile.Length, 2), actualBytes.Skip(7).Take(2).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(1, 1), actualBytes.Skip(9).Take(1).ToArray());
        }

        [Test]
        public void LengthMismatch_ThrowsIOException()
        {
            var bytes = MakeECFFileWithWrongLength(12345678, 5,
                new ECFRecord { ID = 1, Name = "Class1" },
                new ECFRecord { ID = 2, Name = "Class2" },
                new ECFRecord { ID = 3, Name = "Class3" });

            Assert.Throws<IOException>(() => _classFile.DeserializeFromByteArray(bytes, new NumberEncoderService()));
        }

        [Test]
        public void DeserializeFromByteArray_HasExpectedIDAndNames()
        {
            var records = new[]
            {
                new ECFRecord {ID = 1, Name = "TestFixture"},
                new ECFRecord {ID = 2, Name = "Test2"},
                new ECFRecord {ID = 3, Name = "Test3"},
                new ECFRecord {ID = 4, Name = "Test4"},
                new ECFRecord {ID = 5, Name = "Test5"},
                new ECFRecord {ID = 6, Name = "Test6"},
                new ECFRecord {ID = 7, Name = "Test7"},
                new ECFRecord {ID = 8, Name = "Test8"},
                new ECFRecord {ID = 9, Name = "eof"}
            };
            var bytes = MakeECFFile(55565554, records);

            _classFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            CollectionAssert.AreEqual(records.Select(x => new { x.ID, x.Name }).ToList(),
                                      _classFile.Data.Select(x => new { x.ID, x.Name }).ToList());
        }

        private byte[] MakeECFFile(int checksum, params ECFRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("ECF"));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
            bytes.AddRange(numberEncoderService.EncodeNumber(records.Length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);
            foreach (var record in records)
                bytes.AddRange(record.SerializeToByteArray(numberEncoderService));

            return bytes.ToArray();
        }

        private byte[] MakeECFFileWithWrongLength(int checksum, int length, params ECFRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("ECF"));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
            bytes.AddRange(numberEncoderService.EncodeNumber(length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);
            foreach (var record in records)
                bytes.AddRange(record.SerializeToByteArray(numberEncoderService));

            return bytes.ToArray();
        }
    }
}
