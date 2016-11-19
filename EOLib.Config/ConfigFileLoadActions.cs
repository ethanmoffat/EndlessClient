// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Config
{
    public class ConfigFileLoadActions : IConfigFileLoadActions
    {
        private readonly IConfigurationRepository _configRepository;

        public ConfigFileLoadActions(IConfigurationRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public void LoadConfigFile()
        {
            var configFile = new IniReader(ConfigStrings.Default_Config_File);
            if (!configFile.Load())
                throw new ConfigLoadException();

            int tempInt;
            _configRepository.VersionMajor = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Major, out tempInt)
                ? (byte)tempInt
                : ConfigDefaults.MajorVersion;

            _configRepository.VersionMinor = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Minor, out tempInt)
                ? (byte)tempInt
                : ConfigDefaults.MinorVersion;

            _configRepository.VersionBuild = configFile.GetValue(ConfigStrings.Version, ConfigStrings.Client, out tempInt)
                ? (byte)tempInt
                : ConfigDefaults.ClientVersion;

            _configRepository.Language = configFile.GetValue(ConfigStrings.LANGUAGE, ConfigStrings.Language, out tempInt)
                ? (EOLanguage)tempInt
                : EOLanguage.English;

            _configRepository.PlayerDropProtectTime = configFile.GetValue(ConfigStrings.Custom,
                ConfigStrings.PlayerDropProtectTime, out tempInt)
                ? tempInt
                : ConfigDefaults.PlayerDropProtectionSeconds;

            _configRepository.NPCDropProtectTime = configFile.GetValue(ConfigStrings.Custom, ConfigStrings.NPCDropProtectTime,
                out tempInt)
                ? tempInt
                : ConfigDefaults.NPCDropProtectionSeconds;

            bool tempBool;
            _configRepository.CurseFilterEnabled = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.Filter, out tempBool) && tempBool;
            _configRepository.StrictFilterEnabled = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.FilterAll, out tempBool) && tempBool;

            _configRepository.ShowShadows = !configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowShadows, out tempBool) || tempBool;
            _configRepository.ShowTransition = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowTransition, out tempBool) && tempBool;
            _configRepository.MusicEnabled = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.Music, out tempBool) && tempBool;
            _configRepository.SoundEnabled = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.Sound, out tempBool) && tempBool;
            _configRepository.ShowChatBubbles = !configFile.GetValue(ConfigStrings.Settings, ConfigStrings.ShowBaloons, out tempBool) || tempBool;

            _configRepository.EnableLog = configFile.GetValue(ConfigStrings.Settings, ConfigStrings.EnableLogging, out tempBool) && tempBool;
            _configRepository.HearWhispers = !configFile.GetValue(ConfigStrings.Chat, ConfigStrings.HearWhisper, out tempBool) || tempBool;
            _configRepository.Interaction = !configFile.GetValue(ConfigStrings.Chat, ConfigStrings.Interaction, out tempBool) || tempBool;
            _configRepository.LogChatToFile = configFile.GetValue(ConfigStrings.Chat, ConfigStrings.LogChat, out tempBool) && tempBool;

            string host;
            _configRepository.Host = configFile.GetValue(ConfigStrings.Connection, ConfigStrings.Host, out host) ? host : ConfigDefaults.Host;
            _configRepository.Port = configFile.GetValue(ConfigStrings.Connection, ConfigStrings.Port, out tempInt) ? tempInt : ConfigDefaults.Port;
        }
    }
}
