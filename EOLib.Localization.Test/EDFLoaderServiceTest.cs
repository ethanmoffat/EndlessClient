using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using EOLib.IO.Services;
using NUnit.Framework;

namespace EOLib.Localization.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class EDFLoaderServiceTest
    {
        private const string FILE_NAME = "test.edf";

        private IEDFLoaderService _edfLoaderService;

        [SetUp]
        public void SetUp()
        {
            _edfLoaderService = new EDFLoaderService(new DataEncoderService());
        }

        public void TearDown()
        {
            if (File.Exists(FILE_NAME))
                File.Delete(FILE_NAME);
        }

        [Test]
        public void GivenNonExistingFile_ExpectFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => _edfLoaderService.LoadFile("fileThatDoesNotExist", DataFiles.Checksum));
        }

        [Test]
        public void GivenCurseFile_DecodeStringDelimitedByColons()
        {
            const string curseString = "CsusrAs:e5:4C3uErSsReU2C:";
            GivenDataFileWithContents(FILE_NAME, curseString);

            var file = _edfLoaderService.LoadFile(FILE_NAME, DataFiles.CurseFilter);

            var expectedCurses = new[]
            {
                "Curse", "Curse2", "CURSE345", "Ass"
            };

            CollectionAssert.AreEqual(expectedCurses, file.Data.Values);
        }

        [Test]
        public void GivenCurseFile_DecodeStringDelimitedByColons_HandlesMultipleLines()
        {
            const string curseString = "CsusrAs:e5:4C3uErSsReU2C:\nARBQCPDOEN:MF:GLHKIJ:";
            GivenDataFileWithContents(FILE_NAME, curseString);

            var file = _edfLoaderService.LoadFile(FILE_NAME, DataFiles.CurseFilter);

            var expectedCurses = new[]
            {
                "Curse", "Curse2", "CURSE345", "Ass",
                "ABCDE", "FGHI", "JKL", "MNOPQR"
            };

            CollectionAssert.AreEqual(expectedCurses, file.Data.Values);
        }

        [Test]
        public void GivenCreditsFile_DoesNotDecodeFileContents_SplitsByLines()
        {
            const string credits = "Created By\nMe :)\nMe again!";
            GivenDataFileWithContents(FILE_NAME, credits);

            var file = _edfLoaderService.LoadFile(FILE_NAME, DataFiles.Credits);

            var expectedCredits = credits.Split('\n');
            CollectionAssert.AreEqual(expectedCredits, file.Data.Values);
        }

        [Test]
        public void GivenChecksumFile_DoesNotDecodeFileContents()
        {
            const string checksum = "218:13:2:176";
            GivenDataFileWithContents(FILE_NAME, checksum);

            var file = _edfLoaderService.LoadFile(FILE_NAME, DataFiles.Checksum);

            Assert.AreEqual(1, file.Data.Count);
            Assert.AreEqual(checksum, file.Data.Values.Single());
        }

        [Test]
        public void NonSpecialDataFiles_AreDecodedCorrectly()
        {
            const string fileData = "a7b6cg1f2e3d4 5";
            const string expectedString = "abc12345 defg67";

            GivenDataFileWithContents(FILE_NAME, fileData);

            var valuesToTest = ((DataFiles[]) Enum.GetValues(typeof(DataFiles))).Skip(3);
            foreach (var file in valuesToTest)
            {
                var edf = _edfLoaderService.LoadFile(FILE_NAME, file);
                Assert.AreEqual(expectedString, edf.Data.Values.Single());
            }
        }

        [Test]
        public void NonSpecialDataFiles_AreDecodedCorrectly_MultipleLines()
        {
            const string fileData = "a7b6cg1f2e3d4 5\na7b6cg1f2e3d4 5";
            var expectedStrings = new[] { "abc12345 defg67", "abc12345 defg67"};

            GivenDataFileWithContents(FILE_NAME, fileData);

            var valuesToTest = ((DataFiles[])Enum.GetValues(typeof(DataFiles))).Skip(3);
            foreach (var file in valuesToTest)
            {
                var edf = _edfLoaderService.LoadFile(FILE_NAME, file);
                CollectionAssert.AreEqual(expectedStrings, edf.Data.Values);
            }
        }

        [Test]
        public void NonSpecialDataFiles_SwapAdjacentCharacterValues_MultiplesOfSeven()
        {
            //p, p, and i are adjacent multiples of 7 in this example
            //see https://eoserv.net/wiki/EDF_Data_Files for more info
            //the expected string would be Crazy steippng without this step
            const string fileData = "Cgrnapzpyi est";
            var expectedString = "Crazy stepping";

            GivenDataFileWithContents(FILE_NAME, fileData);

            var valuesToTest = ((DataFiles[])Enum.GetValues(typeof(DataFiles))).Skip(3);
            foreach (var file in valuesToTest)
            {
                var edf = _edfLoaderService.LoadFile(FILE_NAME, file);
                Assert.AreEqual(expectedString, edf.Data.Values.Single());
            }
        }

        private void GivenDataFileWithContents(string fileName, string fileData)
        {
            File.WriteAllText(fileName, fileData);
        }
    }
}
