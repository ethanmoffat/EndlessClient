// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test.Pub
{
    [TestClass]
    public class EIFFileTest
    {
        private IPubFile<EIFRecord> _itemFile;

        [TestInitialize]
        public void TestInitialize()
        {
            _itemFile = new EIFFile();
        }

        [TestMethod]
        public void EIFFile_HasCorrectFileType()
        {
            Assert.AreEqual("EIF", _itemFile.FileType);
        }

        [TestMethod]
        public void EIFFile_HasExpectedChecksumAndLength()
        {
            Assert.AreEqual(0, _itemFile.CheckSum);
            Assert.AreEqual(0, _itemFile.Length);
        }

        [TestMethod]
        public void EIFFile_WithOneItemRecord_HasExpectedLength()
        {
            var bytes = MakeEIFFile(1234, new EIFRecord {ID = 1, Name = "TestItem"});

            _itemFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            Assert.AreEqual(1, _itemFile.Length);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EIFFile_Indexing_ThrowsExceptionWhenLessThan1()
        {
            var bytes = MakeEIFFile(1234, 
                                     new EIFRecord { ID = 1, Name = "TestItem" },
                                     new EIFRecord {ID = 2, Name = "Test2"},
                                     new EIFRecord {ID = 3, Name = "Test3"},
                                     new EIFRecord {ID = 4, Name = "Test4"});

            _itemFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            Assert.AreEqual(4, _itemFile.Length);

            var invalidItem = _itemFile[0];
            Assert.IsNotNull(invalidItem);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EIFFile_Indexing_ThrowsExceptionWhenLessThanCount()
        {
            var bytes = MakeEIFFile(1234, 
                                     new EIFRecord { ID = 1, Name = "TestItem" },
                                     new EIFRecord { ID = 2, Name = "Test2" });

            _itemFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            Assert.AreEqual(2, _itemFile.Length);

            var invalidItem = _itemFile[3];
            Assert.IsNotNull(invalidItem);
        }

        [TestMethod]
        public void EIFFile_Indexing_ReturnsExpectedItemWhenRequestedByID()
        {
            var items = new[]
            {
                new EIFRecord {ID = 1, Name = "TestItem"},
                new EIFRecord {ID = 2, Name = "Test2"},
                new EIFRecord {ID = 3, Name = "Test3"},
                new EIFRecord {ID = 4, Name = "Test4"},
                new EIFRecord {ID = 5, Name = "Test5"},
                new EIFRecord {ID = 6, Name = "Test6"},
                new EIFRecord {ID = 7, Name = "Test7"},
                new EIFRecord {ID = 8, Name = "Test8"}
            };

            var bytes = MakeEIFFile(1234, items);

            _itemFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            Assert.AreEqual(items.Length, _itemFile.Length);

            for (int i = 0; i < items.Length; ++i)
                Assert.AreEqual(items[i].Name, _itemFile[items[i].ID].Name, "Failed at index {0}", i);
        }

        [TestMethod]
        public void EIFFile_SerializeToByteArray_ReturnsExpectedBytes()
        {
            var expectedBytes = MakeEIFFile(55565554,
                new EIFRecord {ID = 1, Name = "TestItem"},
                new EIFRecord {ID = 2, Name = "Test2"},
                new EIFRecord {ID = 3, Name = "Test3"},
                new EIFRecord {ID = 4, Name = "Test4"},
                new EIFRecord {ID = 5, Name = "Test5"},
                new EIFRecord {ID = 6, Name = "Test6"},
                new EIFRecord {ID = 7, Name = "Test7"},
                new EIFRecord {ID = 8, Name = "Test8"},
                new EIFRecord {ID = 9, Name = "eof"});

            _itemFile.DeserializeFromByteArray(expectedBytes, new NumberEncoderService());

            var actualBytes = _itemFile.SerializeToByteArray(new NumberEncoderService());

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void EIFFile_HeaderFormat_IsCorrect()
        {
            var nes = new NumberEncoderService();

            var actualBytes = _itemFile.SerializeToByteArray(nes);

            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes(_itemFile.FileType), actualBytes.Take(3).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_itemFile.CheckSum, 4), actualBytes.Skip(3).Take(4).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(_itemFile.Length, 2), actualBytes.Skip(7).Take(2).ToArray());
            CollectionAssert.AreEqual(nes.EncodeNumber(1, 1), actualBytes.Skip(9).Take(1).ToArray());
        }

        private byte[] MakeEIFFile(int checksum, params EIFRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes("EIF"));
            bytes.AddRange(numberEncoderService.EncodeNumber(checksum, 4));
            bytes.AddRange(numberEncoderService.EncodeNumber(records.Length, 2));
            bytes.Add(numberEncoderService.EncodeNumber(1, 1)[0]);
            foreach (var record in records)
                bytes.AddRange(record.SerializeToByteArray(numberEncoderService));

            return bytes.ToArray();
        }
    }
}
