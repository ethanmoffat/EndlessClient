﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using EOLib.Shared;
using NUnit.Framework;

namespace EOLib.Localization.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class DataFileLoadActionsTest
    {
        private IDataFileLoadActions _actions;
        private IDataFileRepository _dataFileRepository;
        private IEDFLoaderService _edfLoaderService;

        [SetUp]
        public void SetUp()
        {
            _dataFileRepository = new DataFileRepository();
            _edfLoaderService = new EDFLoaderService();
            _actions = new DataFileLoadActions(_dataFileRepository, _edfLoaderService);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(Constants.DataFilePath))
                Directory.Delete(Constants.DataFilePath, true);
        }

        [Test]
        public void GivenMissingDataDirectory_WhenLoadDataFiles_ExpectDataFileLoadException()
        {
            Assert.Throws<DataFileLoadException>(() => _actions.LoadDataFiles());
        }

        [Test]
        public void GivenIncorrectNumberOfDataFiles_WhenLoadDataFiles_ExpectDataFileLoadException()
        {
            CreateRequiredDirectory();
            GivenEDFFilesInRequiredDirectory(5, "{0}.edf");

            Assert.Throws<DataFileLoadException>(() => _actions.LoadDataFiles());
        }

        [Test]
        public void WhenLoadDataFiles_DataFilesHaveIncorrectNameFormat_ExpectDataFileLoadException()
        {
            CreateRequiredDirectory();
            GivenEDFFilesInRequiredDirectory(nameFormat: "{0}.edf");

            Assert.Throws<DataFileLoadException>(() => _actions.LoadDataFiles());
        }

        [Test]
        public void WhenLoadDataFiles_RepositoryHasExpectedNumberOfFiles()
        {
            CreateRequiredDirectory();
            GivenEDFFilesInRequiredDirectory();
            _dataFileRepository.DataFiles.Add(DataFiles.Credits, new EDFFile(DataFiles.Credits));

            _actions.LoadDataFiles();

            Assert.That(_dataFileRepository.DataFiles.Count, Is.EqualTo(Constants.ExpectedNumberOfDataFiles));
        }

        private void CreateRequiredDirectory()
        {
            if (!Directory.Exists(Constants.DataFilePath))
                Directory.CreateDirectory(Constants.DataFilePath);
        }

        private void GivenEDFFilesInRequiredDirectory(int numberOfFiles = Constants.ExpectedNumberOfDataFiles,
                                                      string nameFormat = "dat0{0:00}.edf")
        {
            for (int i = 1; i <= numberOfFiles; ++i)
                File.Create(string.Format(Path.Combine(Constants.DataFilePath, nameFormat), i)).Close();
        }
    }
}
