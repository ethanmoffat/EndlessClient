using System;
using System.Collections.Generic;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test.Pub
{
    [TestClass]
    public class BasePubFileTest
    {
        //This covers the BasePubFile abstract class.
        private BasePubFile<DummyRecord> _baseFile;

        [TestInitialize]
        public void TestInitialize()
        {
            _baseFile = new DummyFile();
        }

        [TestMethod]
        public void PubFile_HasExpectedChecksumAndLength()
        {
            Assert.AreEqual(0, _baseFile.CheckSum);
            Assert.AreEqual(0, _baseFile.Length);
        }

        [TestMethod]
        public void PubFile_WithOneItemRecord_HasExpectedLength()
        {
            var bytes = MakeDummyFile(1234, new DummyRecord { ID = 1, Name = "TestItem" });

            _baseFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            Assert.AreEqual(1, _baseFile.Length);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PubFile_Indexing_ThrowsExceptionWhenLessThan1()
        {
            var bytes = MakeDummyFile(1234,
                                     new DummyRecord { ID = 1, Name = "TestItem" },
                                     new DummyRecord { ID = 2, Name = "Test2" },
                                     new DummyRecord { ID = 3, Name = "Test3" },
                                     new DummyRecord { ID = 4, Name = "Test4" });

            _baseFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            Assert.AreEqual(4, _baseFile.Length);

            var invalidItem = _baseFile[0];
            Assert.IsNotNull(invalidItem);
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PubFile_Indexing_ThrowsExceptionWhenLessThanCount()
        {
            var bytes = MakeDummyFile(1234,
                                     new DummyRecord { ID = 1, Name = "TestItem" },
                                     new DummyRecord { ID = 2, Name = "Test2" });

            _baseFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            Assert.AreEqual(2, _baseFile.Length);

            var invalidItem = _baseFile[3];
            Assert.IsNotNull(invalidItem);
        }

        [TestMethod]
        public void PubFile_Indexing_ReturnsExpectedItemWhenRequestedByID()
        {
            var items = new[]
            {
                new DummyRecord {ID = 1, Name = "TestItem"},
                new DummyRecord {ID = 2, Name = "Test2"},
                new DummyRecord {ID = 3, Name = "Test3"},
                new DummyRecord {ID = 4, Name = "Test4"},
                new DummyRecord {ID = 5, Name = "Test5"},
                new DummyRecord {ID = 6, Name = "Test6"},
                new DummyRecord {ID = 7, Name = "Test7"},
                new DummyRecord {ID = 8, Name = "Test8"}
            };

            var bytes = MakeDummyFile(1234, items);

            _baseFile.DeserializeFromByteArray(bytes, new NumberEncoderService());

            Assert.AreEqual(items.Length, _baseFile.Length);

            for (int i = 0; i < items.Length; ++i)
                Assert.AreEqual(items[i].Name, _baseFile[items[i].ID].Name, "Failed at index {0}", i);
        }

        private byte[] MakeDummyFile(int checksum, params DummyRecord[] records)
        {
            var numberEncoderService = new NumberEncoderService();

            var bytes = new List<byte>();

            bytes.Add((byte)records.Length);
            foreach (var record in records)
                bytes.AddRange(record.SerializeToByteArray(numberEncoderService));

            return bytes.ToArray();
        }
    }
}
