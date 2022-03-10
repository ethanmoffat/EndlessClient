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
    public class ENFFileTest
    {
        private IPubFile<ENFRecord> _npcFile;

        [SetUp]
        public void SetUp()
        {
            _npcFile = new ENFFile();
        }

        [Test]
        public void HasCorrectFileType()
        {
            Assert.AreEqual("ENF", _npcFile.FileType);
        }

        [Test]
        public void SerializeToByteArray_ReturnsExpectedBytes()
        {
            var expectedBytes = MakeENFFile(55565554,
                new ENFRecord { ID = 1, Name = "TestNPC" },
                new ENFRecord { ID = 2, Name = "Test2" },
                new ENFRecord { ID = 3, Name = "Test3" },
                new ENFRecord { ID = 4, Name = "Test4" },
                new ENFRecord { ID = 5, Name = "Test5" },
                new ENFRecord { ID = 6, Name = "Test6" },
                new ENFRecord { ID = 7, Name = "Test7" },
                new ENFRecord { ID = 8, Name = "Test8" },
                new ENFRecord { ID = 9, Name = "eof" });

            _npcFile.DeserializeFromByteArray(expectedBytes, new NumberEncoderService());

            var actualBytes = _npcFile.SerializeToByteArray(new NumberEncoderService(), rewriteChecksum: false);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void HeaderFormat_IsCorrect()
        {
            var nes = new NumberEncoderService();

            var actualBytes = _npcFile.SerializeToByteArray(nes, rewriteChecksum: false);

            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes(_npcFile.FileType), actualBytes.Take(3).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_npcFile.CheckSum, 4), actualBytes.Skip(3).Take(4).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_npcFile.Length, 2), actualBytes.Skip(7).Take(2).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(1, 1), actualBytes.Skip(9).Take(1).ToArray());
        }

        [Test]
        public void LengthMismatch_ThrowsIOException()
        {
            var bytes = MakeENFFileWithWrongLength(12345678, 5,
                new ENFRecord { ID = 1, Name = "NPC1" },
                new ENFRecord { ID = 2, Name = "NPC2" },
                new ENFRecord { ID = 3, Name = "NPC3" });

            Assert.Throws<IOException>(() => _npcFile.DeserializeFromByteArray(bytes, new NumberEncoderService()));
        }

        [Test]
        public void DeserializeFromByteArray_HasExpectedIDAndNames()
        {
            var records = new[]
            {
                new ENFRecord {ID = 1, Name = "Test"},
                new ENFRecord {ID = 2, Name = "Test2"},
                new ENFRecord {ID = 3, Name = "Test3"},
                new ENFRecord {ID = 4, Name = "Test4"},
                new ENFRecord {ID = 5, Name = "Test5"},
                new ENFRecord {ID = 6, Name = "Test6"},
                new ENFRecord {ID = 7, Name = "Test7"},
                new ENFRecord {ID = 8, Name = "Test8"},
                new ENFRecord {ID = 9, Name = "eof"}
            };
            var bytes = MakeENFFile(55565554, records);

            _npcFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            CollectionAssert.AreEqual(records.Select(x => new { x.ID, x.Name }).ToList(),
                                      _npcFile.Data.Select(x => new { x.ID, x.Name }).ToList());
        }

        private byte[] MakeENFFile(int checksum, params ENFRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("ENF"));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
            bytes.AddRange(numberEncoderService.EncodeNumber(records.Length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);
            foreach (var record in records)
                bytes.AddRange(record.SerializeToByteArray(numberEncoderService));

            return bytes.ToArray();
        }

        private byte[] MakeENFFileWithWrongLength(int checksum, int length, params ENFRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("ENF"));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
            bytes.AddRange(numberEncoderService.EncodeNumber(length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);
            foreach (var record in records)
                bytes.AddRange(record.SerializeToByteArray(numberEncoderService));

            return bytes.ToArray();
        }
    }
}
