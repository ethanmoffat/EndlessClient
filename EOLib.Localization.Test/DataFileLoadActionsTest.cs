// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.Localization.Test
{
    [TestClass, ExcludeFromCodeCoverage]
    public class DataFileLoadActionsTest
    {
        private IDataFileLoadActions _actions;
        private IDataFileRepository _dataFileRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _dataFileRepository = new DataFileRepository();
            _actions = new DataFileLoadActions(_dataFileRepository);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(DataFileConstants.DataFilePath))
                Directory.Delete(DataFileConstants.DataFilePath, true);
        }

        [TestMethod]
        [ExpectedException(typeof(DataFileLoadException), DataFileLoadException.ExceptionMessage)]
        public void GivenMissingDataDirectory_WhenLoadDataFiles_ExpectDataFileLoadException()
        {
            _actions.LoadDataFiles();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFileLoadException), DataFileLoadException.ExceptionMessage)]
        public void GivenIncorrectNumberOfDataFiles_WhenLoadDataFiles_ExpectDataFileLoadException()
        {
            CreateRequiredDirectory();
            GivenEDFFilesInRequiredDirectory(5, "{0}.edf");

            _actions.LoadDataFiles();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFileLoadException), DataFileLoadException.ExceptionMessage)]
        public void WhenLoadDataFiles_DataFilesHaveIncorrectNameFormat_ExpectDataFileLoadException()
        {
            CreateRequiredDirectory();
            GivenEDFFilesInRequiredDirectory(nameFormat: "{0}.edf");

            _actions.LoadDataFiles();
        }

        [TestMethod]
        public void WhenLoadDataFiles_RepositoryHasExpectedNumberOfFiles()
        {
            CreateRequiredDirectory();
            GivenEDFFilesInRequiredDirectory();
            _dataFileRepository.DataFiles.Add(DataFiles.Credits, new EDFFile("data/dat001.edf", DataFiles.Credits));

            _actions.LoadDataFiles();

            Assert.AreEqual(DataFileConstants.ExpectedNumberOfDataFiles, _dataFileRepository.DataFiles.Count);
        }

        private void CreateRequiredDirectory()
        {
            if (!Directory.Exists(DataFileConstants.DataFilePath))
                Directory.CreateDirectory(DataFileConstants.DataFilePath);
        }

        private void GivenEDFFilesInRequiredDirectory(int numberOfFiles = DataFileConstants.ExpectedNumberOfDataFiles,
                                                      string nameFormat = "dat0{0:00}.edf")
        {
            for (int i = 1; i <= numberOfFiles; ++i)
                File.Create(string.Format(Path.Combine(DataFileConstants.DataFilePath, nameFormat), i)).Close();
        }
    }
}
