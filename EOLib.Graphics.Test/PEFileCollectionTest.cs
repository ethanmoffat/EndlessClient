// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PELoaderLib;

namespace EOLib.Graphics.Test
{
    [TestClass, ExcludeFromCodeCoverage]
    public class PEFileCollectionTest
    {
        private const string ExpectedDirectory = "GFX";

        private IPEFileCollection _collection;

        [TestInitialize]
        public void TestInitialize()
        {
            _collection = new PEFileCollection();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _collection.Dispose();

            if (Directory.Exists(ExpectedDirectory))
                Directory.Delete(ExpectedDirectory, true);
        }

        [TestMethod, ExpectedException(typeof(DirectoryNotFoundException))]
        public void PopulateCollection_ThrowsDirectoryNotFound_ForMissingDirectory()
        {
            _collection.PopulateCollectionWithStandardGFX();
        }

        [TestMethod, ExpectedException(typeof(FileNotFoundException))]
        public void PopulateCollection_ThrowsFileNotFound_ForMissingFilesInDirectory()
        {
            CreateExpectedDirectoryWithFiles(5);
            _collection.PopulateCollectionWithStandardGFX();
        }

        [TestMethod]
        public void Constructor_FillsCollection_WithExpectedPEFiles()
        {
            var expectedKeys = Enum.GetValues(typeof(GFXTypes));
            CreateExpectedDirectoryWithFiles(25);

            _collection.PopulateCollectionWithStandardGFX();

            Assert.AreEqual(25, _collection.Count);
            CollectionAssert.AreEqual(expectedKeys, _collection.Keys.ToList());
        }

        [TestMethod]
        public void Dispose_DisposesAllFiles()
        {
            _collection.Add(GFXTypes.PreLoginUI, Mock.Of<IPEFile>());
            _collection.Add(GFXTypes.PostLoginUI, Mock.Of<IPEFile>());

            _collection.Dispose();

            foreach(var file in _collection.Values)
                Mock.Get(file).Verify(x => x.Dispose(), Times.Once);
        }

        private void CreateExpectedDirectoryWithFiles(int numFiles = 0, string fileNameFormat = "gfx{0:D3}.egf")
        {
            Directory.CreateDirectory(ExpectedDirectory);
            for (int i = 1; i <= numFiles; ++i)
                File.WriteAllText(string.Format(Path.Combine(ExpectedDirectory, fileNameFormat), i), "test contents");
        }
    }
}
