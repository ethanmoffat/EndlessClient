using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using EOLib.Shared;
using Moq;
using NUnit.Framework;
using PELoaderLib;

namespace EOLib.Graphics.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class PEFileCollectionTest
    {
        private static readonly string _expectedDirectory = PathResolver.GetPath("gfx");
        private static readonly string _expectedDirectoryRoot = _expectedDirectory.Split(Path.DirectorySeparatorChar)[0];

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

            if (Directory.Exists(_expectedDirectoryRoot))
                Directory.Delete(_expectedDirectoryRoot, recursive: true);
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

            Assert.That(_collection, Has.Count.EqualTo(25));
            Assert.That(_collection.Keys, Is.EqualTo(expectedKeys));
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

        private static void CreateExpectedDirectoryWithFiles(int numFiles = 0, string fileNameFormat = "gfx{0:D3}.egf")
        {
            var components = _expectedDirectory.Split(Path.DirectorySeparatorChar);
            foreach (var component in components)
                Directory.CreateDirectory(_expectedDirectory);

            for (int i = 1; i <= numFiles; ++i)
            {
                File.WriteAllText(string.Format(Path.Combine(_expectedDirectory, fileNameFormat), i), "test contents");
            }
        }
    }
}
