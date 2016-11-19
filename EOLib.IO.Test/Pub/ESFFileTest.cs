// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test.Pub
{
    [TestClass, ExcludeFromCodeCoverage]
    public class ESFFileTest
    {
        private IPubFile<ESFRecord> _spellFile;

        [TestInitialize]
        public void TestInitialize()
        {
            _spellFile = new ESFFile();
        }

        [TestMethod]
        public void ESFFile_HasCorrectFileType()
        {
            Assert.AreEqual("ESF", _spellFile.FileType);
        }

        [TestMethod]
        public void ESFFile_SerializeToByteArray_ReturnsExpectedBytes()
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

        [TestMethod]
        public void ESFFile_HeaderFormat_IsCorrect()
        {
            var nes = new NumberEncoderService();

            var actualBytes = _spellFile.SerializeToByteArray(nes);

            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes(_spellFile.FileType), actualBytes.Take(3).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_spellFile.CheckSum, 4), actualBytes.Skip(3).Take(4).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_spellFile.Length, 2), actualBytes.Skip(7).Take(2).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(1, 1), actualBytes.Skip(9).Take(1).ToArray());
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
    }
}
