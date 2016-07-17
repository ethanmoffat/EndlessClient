// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.IO.Repositories;

namespace EOLib.IO.Services
{
    public class LocalizedStringService : ILocalizedStringService
    {
        private readonly IConfigurationProvider _configProvider;
        private readonly IDataFileProvider _dataFileProvider;

        public LocalizedStringService(IConfigurationProvider configProvider,
                                      IDataFileProvider dataFileProvider)
        {
            _configProvider = configProvider;
            _dataFileProvider = dataFileProvider;
        }

        public string GetString(EOLanguage language, DATCONST1 dataConstant)
        {
            return _dataFileProvider
                .DataFiles[GetFile1FromLanguage(language)]
                .Data[(int)dataConstant];
        }

        public string GetString(EOLanguage language, DATCONST2 dataConstant)
        {
            return _dataFileProvider
                .DataFiles[GetFile2FromLanguage(language)]
                .Data[(int)dataConstant];
        }

        public string GetString(DATCONST1 dataConstant)
        {
            return GetString(_configProvider.Language, dataConstant);
        }

        public string GetString(DATCONST2 dataConstant)
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
}
