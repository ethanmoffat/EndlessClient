// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EOLib.Config;
using NUnit.Framework;
using Moq;

namespace EOLib.Localization.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class LocalizedStringFinderTest
    {
        private IConfigurationProvider _configurationProvider;
        private DataFileRepository _dataFileProvider;

        private ILocalizedStringFinder _localizedStringFinder;

        [SetUp]
        public void SetUp()
        {
            _configurationProvider = Mock.Of<IConfigurationProvider>();
            _dataFileProvider = new DataFileRepository();

            _localizedStringFinder = new LocalizedStringFinder(
                _configurationProvider,
                _dataFileProvider);
        }

        [Test]
        public void GetString_Dialog_InvalidLanguage_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _localizedStringFinder.GetString((EOLanguage) 50, DialogResourceID.ACCOUNT_CREATE_ACCEPTED));
        }

        [Test]
        public void GetString_Resource_InvalidLanguage_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _localizedStringFinder.GetString((EOLanguage)50, EOResourceID.STRING_SERVER));
        }

        [Test]
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

        [Test]
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

        [Test]
        public void GetString_DialogID_UsesLanguageSetInConfig()
        {
            const DialogResourceID testID = DialogResourceID.ACCOUNT_CREATE_ACCEPTED;
            const string expectedResourceString = "language test";

            GivenLanguageSetInConfig(EOLanguage.Dutch);
            GivenFileHasStringForResourceID(DataFiles.DutchStatus1, testID, expectedResourceString);

            var actualString = _localizedStringFinder.GetString(testID);

            Assert.AreEqual(expectedResourceString, actualString);
        }

        [Test]
        public void GetString_ResourceID_UsesLanguageSetInConfig()
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
            if (!_dataFileProvider.DataFiles.ContainsKey(file))
                _dataFileProvider.DataFiles.Add(file, Mock.Of<IEDFFile>(x => x.Data == new Dictionary<int, string>()));

            _dataFileProvider.DataFiles[file].Data[(int)id] = str;
        }

        private void GivenFileHasStringForResourceID(DataFiles file, EOResourceID id, string str)
        {
            if (!_dataFileProvider.DataFiles.ContainsKey(file))
                _dataFileProvider.DataFiles.Add(file, Mock.Of<IEDFFile>(x => x.Data == new Dictionary<int, string>()));

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
