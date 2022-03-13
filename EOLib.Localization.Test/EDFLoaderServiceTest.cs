using System;
using System.Collections.Generic;
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
        public void GivenNonExistingFile_Load_ExpectFileNotFoundException()
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
        public void GivenCurseFile_EncodeStringDelimitedByColons()
        {
            const string ExpectedCurseString = "CsusrAs:e5:4C3uErSsReU2C:";

            var curses = new[] { "Curse", "Curse2", "CURSE345", "Ass" };
            var edfFile = new EDFFile(DataFiles.CurseFilter, new Dictionary<int, string>(curses.Select((x, i) => KeyValuePair.Create(i, x))));

            _edfLoaderService.SaveFile(FILE_NAME, edfFile);

            AssertFileContent(FILE_NAME, ExpectedCurseString);
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

        [TestCase(DataFiles.Credits, "Created By\nMe :)\nMe again!")]
        [TestCase(DataFiles.Checksum, "218:13:2:176")]
        public void GivenUnencodedFile_DoesNotDecodeFileContents_SplitsByLines(DataFiles whichFile, string content)
        {
            GivenDataFileWithContents(FILE_NAME, content);

            var file = _edfLoaderService.LoadFile(FILE_NAME, whichFile);

            var expectedLines = content.Split('\n');
            CollectionAssert.AreEqual(expectedLines, file.Data.Values);
        }

        [TestCase(DataFiles.Credits, new[] { "Created By", "Me :)", "Me again!" })]
        [TestCase(DataFiles.Checksum, new[] { "218:13:2:176" })]
        public void GivenUnencodedFile_DoesNotEncodeFileContents_SplitsByDataKeys(DataFiles whichFile, string[] content)
        {
            var edfFile = new EDFFile(whichFile, new Dictionary<int, string>(content.Select((x, i) => KeyValuePair.Create(i, x))));

            _edfLoaderService.SaveFile(FILE_NAME, edfFile);

            AssertFileContent(FILE_NAME, string.Join(Environment.NewLine, content));
        }

        [TestCaseSource(nameof(GetStandardEDFFiles))]
        public void NonSpecialDataFiles_AreDecodedCorrectly(DataFiles whichFile)
        {
            const string fileData = "a7b6cg1f2e3d4 5";
            const string expectedString = "abc12345 defg67";

            GivenDataFileWithContents(FILE_NAME, fileData);

            var edf = _edfLoaderService.LoadFile(FILE_NAME, whichFile);
            Assert.AreEqual(expectedString, edf.Data.Values.Single());
        }

        [TestCaseSource(nameof(GetStandardEDFFiles))]
        public void NonSpecialDataFiles_AreDecodedCorrectly_MultipleLines(DataFiles whichFile)
        {
            const string fileData = "a7b6cg1f2e3d4 5\na7b6cg1f2e3d4 5";
            var expectedStrings = new[] { "abc12345 defg67", "abc12345 defg67"};

            GivenDataFileWithContents(FILE_NAME, fileData);

            var edf = _edfLoaderService.LoadFile(FILE_NAME, whichFile);
            CollectionAssert.AreEqual(expectedStrings, edf.Data.Values);
        }

        [TestCaseSource(nameof(GetStandardEDFFiles))]
        public void NonSpecialDataFiles_Decode_SwapAdjacentCharacterValues_MultiplesOfSeven(DataFiles whichFile)
        {
            //p, p, and i are adjacent multiples of 7 in this example
            //see https://eoserv.net/wiki/EDF_Data_Files for more info
            //the expected string would be Crazy steippng without this step
            const string fileData = "Cgrnapzpyi est";
            var expectedString = "Crazy stepping";

            GivenDataFileWithContents(FILE_NAME, fileData);

            var edf = _edfLoaderService.LoadFile(FILE_NAME, whichFile);
            Assert.AreEqual(expectedString, edf.Data.Values.Single());
        }

        [TestCaseSource(nameof(GetStandardEDFFiles))]
        public void NonSpecialDataFiles_AreEncodedCorrectly(DataFiles whichFile)
        {
            const string ExpectedFileData = "a7b6cg1f2e3d4 5";

            const string Input = "abc12345 defg67";
            var edfFile = new EDFFile(whichFile, new Dictionary<int, string> { { 0, Input } });

            _edfLoaderService.SaveFile(FILE_NAME, edfFile);

            AssertFileContent(FILE_NAME, ExpectedFileData);
        }

        [TestCaseSource(nameof(GetStandardEDFFiles))]
        public void NonSpecialDataFiles_AreEncodedCorrectly_MultipleLines(DataFiles whichFile)
        {
            var expectedFileData = $"a7b6cg1f2e3d4 5{Environment.NewLine}a7b6cg1f2e3d4 5";

            var decodedStrings = new Dictionary<int, string> { { 0, "abc12345 defg67" }, { 1, "abc12345 defg67" } };
            var edfFile = new EDFFile(whichFile, decodedStrings);

            _edfLoaderService.SaveFile(FILE_NAME, edfFile);

            AssertFileContent(FILE_NAME, expectedFileData);
        }

        [TestCaseSource(nameof(GetStandardEDFFiles))]
        public void NonSpecialDataFiles_Encode_SwapAdjacentCharacterValues_MultiplesOfSeven(DataFiles whichFile)
        {
            const string ExpectedFileData = "Cgrnapzpyi est";

            const string Input = "Crazy stepping";
            var edfFile = new EDFFile(whichFile, new Dictionary<int, string> { { 0, Input } });

            _edfLoaderService.SaveFile(FILE_NAME, edfFile);

            AssertFileContent(FILE_NAME, ExpectedFileData);
        }

        public static DataFiles[] GetStandardEDFFiles()
        {
            return new []
            {
                DataFiles.JukeBoxSongs,
                DataFiles.EnglishStatus1,
                DataFiles.EnglishStatus2,
                DataFiles.DutchStatus1,
                DataFiles.DutchStatus2,
                DataFiles.SwedishStatus1,
                DataFiles.SwedishStatus2,
                DataFiles.PortugueseStatus1,
                DataFiles.PortugueseStatus2
            };
        }

        private void GivenDataFileWithContents(string fileName, string fileData)
        {
            File.WriteAllText(fileName, fileData);
        }

        private void AssertFileContent(string fileName, string fileData)
        {
            var text = File.ReadAllText(fileName).Trim();
            Assert.That(text, Is.EqualTo(fileData));
        }
    }
}
