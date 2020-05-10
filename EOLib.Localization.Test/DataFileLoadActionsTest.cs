using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace EOLib.Localization.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class DataFileLoadActionsTest
    {
        private IDataFileLoadActions _actions;
        private IDataFileRepository _dataFileRepository;

        [SetUp]
        public void SetUp()
        {
            _dataFileRepository = new DataFileRepository();
            _actions = new DataFileLoadActions(_dataFileRepository);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(DataFileConstants.DataFilePath))
                Directory.Delete(DataFileConstants.DataFilePath, true);
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
