// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EOLib.Localization.Test
{
    [TestClass]
    public class LocalizedStringFinderTest
    {
        private readonly Dictionary<DataFiles, IEDFFile> _files = new Dictionary<DataFiles, IEDFFile>();
        private IConfigurationProvider _configurationProvider;
        private IDataFileProvider _dataFileProvider;

        private ILocalizedStringFinder _localizedStringFinder;

        [TestInitialize]
        public void TestInitialize()
        {
            _configurationProvider = Mock.Of<IConfigurationProvider>();
            _dataFileProvider = Mock.Of<IDataFileProvider>(x => x.DataFiles == _files);

            _localizedStringFinder = new LocalizedStringFinder(
                _configurationProvider,
                _dataFileProvider);
        }

        [TestMethod]
        public void GetString_DialogID_ByLanguage_MapsToCorrectFile()
        {
            const DialogResourceID testID = DialogResourceID.ACCOUNT_CREATE_ACCEPTED;

            GivenFileHasStringForResourceID(DataFiles.DutchStatus1, testID, "dutch");
            GivenFileHasStringForResourceID(DataFiles.EnglishStatus1, testID, "english");
            GivenFileHasStringForResourceID(DataFiles.PortugueseStatus1, testID, "portuguese");
            GivenFileHasStringForResourceID(DataFiles.SwedishStatus1, testID, "swedish");

            var dutchActual = _localizedStringFinder.GetString(EOLanguage.Dutch, testID);
            var englishActual = _localizedStringFinder.GetString(EOLanguage.English, testID);
            var portugueseActual = _localizedStringFinder.GetString(EOLanguage.Portuguese, testID);
            var swedishActual = _localizedStringFinder.GetString(EOLanguage.Swedish, testID);

            Assert.AreEqual("dutch", dutchActual);
            Assert.AreEqual("english", englishActual);
            Assert.AreEqual("portuguese", portugueseActual);
            Assert.AreEqual("swedish", swedishActual);
        }

        [TestMethod]
        public void GetString_ResourceID_ByLanguage_MapsToCorrectFile()
        {
            const EOResourceID testID = EOResourceID.STATUS_LABEL_YOU_GAINED_EXP;

            GivenFileHasStringForResourceID(DataFiles.DutchStatus2, testID, "dutch");
            GivenFileHasStringForResourceID(DataFiles.EnglishStatus2, testID, "english");
            GivenFileHasStringForResourceID(DataFiles.PortugueseStatus2, testID, "portuguese");
            GivenFileHasStringForResourceID(DataFiles.SwedishStatus2, testID, "swedish");

            var dutchActual = _localizedStringFinder.GetString(EOLanguage.Dutch, testID);
            var englishActual = _localizedStringFinder.GetString(EOLanguage.English, testID);
            var portugueseActual = _localizedStringFinder.GetString(EOLanguage.Portuguese, testID);
            var swedishActual = _localizedStringFinder.GetString(EOLanguage.Swedish, testID);

            Assert.AreEqual("dutch", dutchActual);
            Assert.AreEqual("english", englishActual);
            Assert.AreEqual("portuguese", portugueseActual);
            Assert.AreEqual("swedish", swedishActual);
        }

        [TestMethod]
        public void GetString_UsesLanguageSetInConfig()
        {
            const EOResourceID testID = EOResourceID.STRING_SERVER;
            const string expectedResourceString = "language test";

            GivenLanguageSetInConfig(EOLanguage.Dutch);
            GivenFileHasStringForResourceID(DataFiles.DutchStatus2, testID, expectedResourceString);

            var actualString = _localizedStringFinder.GetString(testID);

            Assert.AreEqual(expectedResourceString, actualString);
        }

        private void GivenFileHasStringForResourceID(DataFiles file, DialogResourceID id, string str)
        {
            if (!_files.ContainsKey(file))
                _files.Add(file, Mock.Of<IEDFFile>(x => x.Data == new Dictionary<int, string>()));

            _files[file].Data[(int) id] = str;
        }

        private void GivenFileHasStringForResourceID(DataFiles file, EOResourceID id, string str)
        {
            if (!_files.ContainsKey(file))
                _files.Add(file, Mock.Of<IEDFFile>(x => x.Data == new Dictionary<int, string>()));

            _dataFileProvider.DataFiles[file].Data[(int)id] = str;
        }

        private void GivenLanguageSetInConfig(EOLanguage language)
        {
            Mock.Get(_configurationProvider)
                .Setup(x => x.Language)
                .Returns(language);
        }
    }
}
