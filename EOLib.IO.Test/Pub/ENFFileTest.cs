// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test.Pub
{
    [TestClass]
    public class ENFFileTest
    {
        private IPubFile<ENFRecord> _npcFile;

        [TestInitialize]
        public void TestInitialize()
        {
            _npcFile = new ENFFile();
        }

        [TestMethod]
        public void ENFFile_HasCorrectFileType()
        {
            Assert.AreEqual("ENF", _npcFile.FileType);
        }

        [TestMethod]
        public void ENFFile_SerializeToByteArray_ReturnsExpectedBytes()
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

            var actualBytes = _npcFile.SerializeToByteArray(new NumberEncoderService());

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void ENFFile_HeaderFormat_IsCorrect()
        {
            var nes = new NumberEncoderService();

            var actualBytes = _npcFile.SerializeToByteArray(nes);

            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes(_npcFile.FileType), actualBytes.Take(3).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_npcFile.CheckSum, 4), actualBytes.Skip(3).Take(4).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_npcFile.Length, 2), actualBytes.Skip(7).Take(2).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(1, 1), actualBytes.Skip(9).Take(1).ToArray());
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
    }
}
