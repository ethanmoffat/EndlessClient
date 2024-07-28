using Moq;
using NUnit.Framework;
using PELoaderLib;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace EOLib.Graphics.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class PEFileCollectionTest
    {
        private const string ExpectedDirectory = "gfx";

        private IPEFileCollection _collection;

        [SetUp]
        public void SetUp()
        {
            _collection = new PEFileCollection();
        }

        [TearDown]
        public void TearDown()
        {
            _collection.Dispose();

            if (Directory.Exists(ExpectedDirectory))
                Directory.Delete(ExpectedDirectory, true);
        }

        [Test]
        public void PopulateCollection_ThrowsDirectoryNotFound_ForMissingDirectory()
        {
            Assert.Throws<DirectoryNotFoundException>(() => _collection.PopulateCollectionWithStandardGFX());
        }

        [Test]
        public void PopulateCollection_ThrowsFileNotFound_ForMissingFilesInDirectory()
        {
            CreateExpectedDirectoryWithFiles(5);
            Assert.Throws<FileNotFoundException>(() => _collection.PopulateCollectionWithStandardGFX());
        }

        [Test]
        public void Constructor_FillsCollection_WithExpectedPEFiles()
        {
            var expectedKeys = Enum.GetValues(typeof(GFXTypes));
            CreateExpectedDirectoryWithFiles(25);

            _collection.PopulateCollectionWithStandardGFX();

            Assert.AreEqual(25, _collection.Count);
            CollectionAssert.AreEqual(expectedKeys, _collection.Keys.ToList());
        }

        [Test]
        public void Dispose_DisposesAllFiles()
        {
            _collection.Add(GFXTypes.PreLoginUI, Mock.Of<IPEFile>());
            _collection.Add(GFXTypes.PostLoginUI, Mock.Of<IPEFile>());

            _collection.Dispose();

            foreach (var file in _collection.Values)
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