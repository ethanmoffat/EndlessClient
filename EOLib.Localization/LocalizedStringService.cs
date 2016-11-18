// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Config;

namespace EOLib.Localization
{
    public class LocalizedStringFinder : ILocalizedStringFinder
    {
        private readonly IConfigurationProvider _configProvider;
        private readonly IDataFileProvider _dataFileProvider;

        public LocalizedStringFinder(IConfigurationProvider configProvider,
                                      IDataFileProvider dataFileProvider)
        {
            _configProvider = configProvider;
            _dataFileProvider = dataFileProvider;
        }

        public string GetString(EOLanguage language, DialogResourceID dataConstant)
        {
            return _dataFileProvider
                .DataFiles[GetFile1FromLanguage(language)]
                .Data[(int)dataConstant];
        }

        public string GetString(EOLanguage language, EOResourceID dataConstant)
        {
            return _dataFileProvider
                .DataFiles[GetFile2FromLanguage(language)]
                .Data[(int)dataConstant];
        }

        public string GetString(DialogResourceID dataConstant)
        {
            return GetString(_configProvider.Language, dataConstant);
        }

        public string GetString(EOResourceID dataConstant)
        {
            return GetString(_configProvider.Language, dataConstant);
        }

        private DataFiles GetFile1FromLanguage(EOLanguage language)
        {
            switch (language)
            {
                case EOLanguage.English: return DataFiles.EnglishStatus1;
                case EOLanguage.Dutch: return DataFiles.DutchStatus1;
                case EOLanguage.Swedish: return DataFiles.SwedishStatus1;
                case EOLanguage.Portuguese: return DataFiles.PortugueseStatus1;
                default: throw new ArgumentOutOfRangeException("language", language, null);
            }
        }

        private DataFiles GetFile2FromLanguage(EOLanguage language)
        {
            switch (language)
            {
                case EOLanguage.English: return DataFiles.EnglishStatus2;
                case EOLanguage.Dutch: return DataFiles.DutchStatus2;
                case EOLanguage.Swedish: return DataFiles.SwedishStatus2;
                case EOLanguage.Portuguese: return DataFiles.PortugueseStatus2;
                default: throw new ArgumentOutOfRangeException("language", language, null);
            }
        }
    }

    public interface ILocalizedStringFinder
    {
        string GetString(EOLanguage language, DialogResourceID dataConstant);
        string GetString(EOLanguage langauge, EOResourceID dataConstant);

        string GetString(DialogResourceID dataConstant);
        string GetString(EOResourceID dataConstant);
    }
}
