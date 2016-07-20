// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace EOLib.IO.Actions
{
    public class FileLoadActions : IFileLoadActions
    {
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IDataFileRepository _dataFileRepository;
        private readonly IConfigurationRepository _configRepository;
        private readonly IMapFileLoadService _mapFileLoadService;

        public FileLoadActions(IMapFileRepository mapFileRepository,
                               IDataFileRepository dataFileRepository,
                               IConfigurationRepository configRepository,
                               IMapFileLoadService mapFileLoadService)
        {
            _mapFileRepository = mapFileRepository;
            _dataFileRepository = dataFileRepository;
            _configRepository = configRepository;
            _mapFileLoadService = mapFileLoadService;
        }

        public void LoadMapFileByID(int id)
        {
            LoadMapFileByName(string.Format(Constants.MapFileFormatString, id));
        }

        public void LoadMapFileByName(string fileName)
        {
            var mapFile = _mapFileLoadService.LoadMapByPath(fileName);
            if (_mapFileRepository.MapFiles.ContainsKey(mapFile.Properties.MapID))
                _mapFileRepository.MapFiles[mapFile.Properties.MapID] = mapFile;
            else
                _mapFileRepository.MapFiles.Add(mapFile.Properties.MapID, mapFile);
        }

        public void LoadDataFiles()
        {
            var files = Directory.GetFiles(Constants.DataFilePath, "*.edf");

            if (!Directory.Exists(Constants.DataFilePath) ||
                files.Length != Constants.ExpectedNumberOfDataFiles)
                throw new DataFileLoadException();

            _dataFileRepository.DataFiles.Clear();
            for (int i = 1; i <= Constants.ExpectedNumberOfDataFiles; ++i)
            {
                if (!DataFileNameIsValid(i, files[i-1]))
                    throw new DataFileLoadException();

                var fileToLoad = (DataFiles) i;
                var loadedFile = new EDFFile(files[i-1], fileToLoad);
                
                _dataFileRepository.DataFiles.Add(fileToLoad, loadedFile);
            }
        }

        public void LoadConfigFile()
        {
            var configFile = new IniReader(ConfigStrings.Default_Config_File);
            if (!configFile.Load())
                throw new ConfigLoadException();

            int tempInt;
            _configRepository.VersionMajor = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Major, out tempInt)
                ? (byte) tempInt
                : Constants.MajorVersion;

            _configRepository.VersionMinor = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Minor, out tempInt)
                ? (byte) tempInt
                : Constants.MinorVersion;

            _configRepository.VersionBuild = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Client, out tempInt)
                ? (byte) tempInt
                : Constants.ClientVersion;

            _configRepository.Language = configFile.GetValue(ConfigStrings.LANGUAGE, ConfigStrings.Language, out tempInt)
                ? (EOLanguage) tempInt
                : EOLanguage.English;

            _configRepository.PlayerDropProtectTime = configFile.GetValue(ConfigStrings.Custom,
                ConfigStrings.PlayerDropProtectTime, out tempInt)
                ? tempInt
                : Constants.PlayerDropProtectionSeconds;

            _configRepository.NPCDropProtectTime = configFile.GetValue(ConfigStrings.Custom, ConfigStrings.NPCDropProtectTime,
                out tempInt)
                ? tempInt
                : Constants.NPCDropProtectionSeconds;

            bool tempBool;
            _configRepository.CurseFilterEnabled = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.Filter, out tempBool) && tempBool;
            _configRepository.StrictFilterEnabled = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.FilterAll, out tempBool) && tempBool;

            _configRepository.ShowShadows = !configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowShadows, out tempBool) || tempBool;
            _configRepository.ShowTransition = !configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowTransition, out tempBool) || tempBool;
            _configRepository.MusicEnabled = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.Music, out tempBool) && tempBool;
            _configRepository.SoundEnabled = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.Sound, out tempBool) && tempBool;
            _configRepository.ShowChatBubbles = !configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowBaloons, out tempBool) || tempBool;

            _configRepository.EnableLog = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.EnableLogging, out tempBool) && tempBool;
            _configRepository.HearWhispers = !configFile.GetValue(ConfigStrings.Chat, ConfigStrings.HearWhisper, out tempBool) || tempBool;
            _configRepository.Interaction = !configFile.GetValue(ConfigStrings.Chat, ConfigStrings.Interaction, out tempBool) || tempBool;
            _configRepository.LogChatToFile = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.LogChat, out tempBool) && tempBool;

            string host;
            _configRepository.Host = configFile.GetValue(ConfigStrings.Connection, ConfigStrings.Host, out host) ? host : Constants.Host;
            _configRepository.Port = configFile.GetValue(ConfigStrings.Connection, ConfigStrings.Port, out tempInt) ? tempInt : Constants.Port;
        }

        private bool DataFileNameIsValid(int fileNumber, string fileName)
        {
            var expectedFormat = string.Format("data/dat0{0:00}.edf", fileNumber);
            return expectedFormat == fileName;
        }
    }
}
