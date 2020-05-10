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
    public class ESFFileTest
    {
        private IPubFile<ESFRecord> _spellFile;

        [SetUp]
        public void SetUp()
        {
            _spellFile = new ESFFile();
        }

        [Test]
        public void HasCorrectFileType()
        {
            Assert.AreEqual("ESF", _spellFile.FileType);
        }

        [Test]
        public void SerializeToByteArray_ReturnsExpectedBytes()
        {
            var expectedBytes = MakeESFFile(55565554,
                new ESFRecord { ID = 1, Name = "TestSpell", Shout = "TestShout" },
                new ESFRecord { ID = 2, Name = "Test2", Shout = "TestShout2" },
                new ESFRecord { ID = 3, Name = "Test3", Shout = "TestShout3" },
                new ESFRecord { ID = 4, Name = "Test4", Shout = "TestShout4" },
                new ESFRecord { ID = 5, Name = "Test5", Shout = "TestShout5" },
                new ESFRecord { ID = 6, Name = "Test6", Shout = "TestShout6" },
                new ESFRecord { ID = 7, Name = "Test7", Shout = "TestShout7" },
                new ESFRecord { ID = 8, Name = "Test8", Shout = "TestShout8" },
                new ESFRecord { ID = 9, Name = "eof", Shout = "-" });

            _spellFile.DeserializeFromByteArray(expectedBytes, new NumberEncoderService());

            var actualBytes = _spellFile.SerializeToByteArray(new NumberEncoderService());

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void HeaderFormat_IsCorrect()
        {
            var nes = new NumberEncoderService();

            var actualBytes = _spellFile.SerializeToByteArray(nes);

            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes(_spellFile.FileType), actualBytes.Take(3).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_spellFile.CheckSum, 4), actualBytes.Skip(3).Take(4).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_spellFile.Length, 2), actualBytes.Skip(7).Take(2).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(1, 1), actualBytes.Skip(9).Take(1).ToArray());
        }

        [Test]
        public void LengthMismatch_ThrowsIOException()
        {
            var bytes = MakeESFFileWithWrongLength(12345678, 5,
                new ESFRecord { ID = 1, Name = "Spell1", Shout = "Spell1" },
                new ESFRecord { ID = 2, Name = "Spell2", Shout = "Spell2" },
                new ESFRecord { ID = 3, Name = "Spell3", Shout = "Spell3" });

            Assert.Throws<IOException>(() => _spellFile.DeserializeFromByteArray(bytes, new NumberEncoderService()));
        }

        [Test]
        public void DeserializeFromByteArray_HasExpectedIDAndNames()
        {
            var records = new[]
            {
                new ESFRecord {ID = 1, Name = "Test", Shout = "Test"},
                new ESFRecord {ID = 2, Name = "Test2", Shout = "Test2"},
                new ESFRecord {ID = 3, Name = "Test3", Shout = "Test3"},
                new ESFRecord {ID = 4, Name = "Test4", Shout = "Test4"},
                new ESFRecord {ID = 5, Name = "Test5", Shout = "Test5"},
                new ESFRecord {ID = 6, Name = "Test6", Shout = "Test6"},
                new ESFRecord {ID = 7, Name = "Test7", Shout = "Test7"},
                new ESFRecord {ID = 8, Name = "Test8", Shout = "Test8"},
                new ESFRecord {ID = 9, Name = "eof", Shout = ""}
            };
            var bytes = MakeESFFile(55565554, records);

            _spellFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            CollectionAssert.AreEqual(records.Select(x => new { x.ID, x.Name }).ToList(),
                                      _spellFile.Data.Select(x => new { x.ID, x.Name }).ToList());
        }

        private byte[] MakeESFFile(int checksum, params ESFRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("ESF"));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
            bytes.AddRange(numberEncoderService.EncodeNumber(records.Length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);
            foreach (var record in records)
                bytes.AddRange(record.SerializeToByteArray(numberEncoderService));

            return bytes.ToArray();
        }

        private byte[] MakeESFFileWithWrongLength(int checksum, int length, params ESFRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("ESF"));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
            bytes.AddRange(numberEncoderService.EncodeNumber(length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);
            foreach (var record in records)
                bytes.AddRange(record.SerializeToByteArray(numberEncoderService));

            return bytes.ToArray();
        }
    }
}
