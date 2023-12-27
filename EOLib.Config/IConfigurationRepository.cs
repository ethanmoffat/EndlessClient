using AutomaticTypeMapper;
using System;

namespace EOLib.Config
{
    public interface IConfigurationRepository
    {
        string Host { get; set; }
        int Port { get; set; }

        int VersionMajor { get; set; }
        int VersionMinor { get; set; }
        int VersionBuild { get; set; }

        EOLanguage Language { get; set; }

        bool CurseFilterEnabled { get; set; }
        bool StrictFilterEnabled { get; set; }

        bool ShowShadows { get; set; }
        bool ShowChatBubbles { get; set; }
        bool ShowTransition { get; set; }
        int PlayerDropProtectTime { get; set; }
        int NPCDropProtectTime { get; set; }

        bool MusicEnabled { get; set; }
        bool SoundEnabled { get; set; }

        bool HearWhispers { get; set; }
        bool Interaction { get; set; }
        bool LogChatToFile { get; set; }

        TimeSpan AccountCreateTimeout { get; set; }

        bool EnableLog { get; set; }

        int InGameWidth { get; set; }
        int InGameHeight { get; set; }

        bool DebugCrashes { get; set; }
    }

    public interface IConfigurationProvider
    {
        string Host { get; }
        int Port { get; }

        int VersionMajor { get; }
        int VersionMinor { get; }
        int VersionBuild { get; }

        EOLanguage Language { get; }

        bool CurseFilterEnabled { get; }
        bool StrictFilterEnabled { get; }

        bool ShowShadows { get; }
        bool ShowChatBubbles { get; }
        bool ShowTransition { get; }
        int PlayerDropProtectTime { get; }
        int NPCDropProtectTime { get; }

        bool MusicEnabled { get; }
        bool SoundEnabled { get; }

        bool HearWhispers { get; }
        bool Interaction { get; }
        bool LogChatToFile { get; }

        TimeSpan AccountCreateTimeout { get; }

        bool EnableLog { get; }

        int InGameWidth { get; }
        int InGameHeight { get; }

        bool DebugCrashes { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ConfigurationRepository : IConfigurationRepository, IConfigurationProvider
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public int VersionBuild { get; set; }

        public EOLanguage Language { get; set; }

        public bool CurseFilterEnabled { get; set; }
        public bool StrictFilterEnabled { get; set; }

        public bool ShowShadows { get; set; }
        public bool ShowChatBubbles { get; set; }
        public bool ShowTransition { get; set; }
        public int PlayerDropProtectTime { get; set; }
        public int NPCDropProtectTime { get; set; }

        public bool MusicEnabled { get; set; }
        public bool SoundEnabled { get; set; }

        public bool HearWhispers { get; set; }
        public bool Interaction { get; set; }
        public bool LogChatToFile { get; set; }

        public TimeSpan AccountCreateTimeout { get; set; }

        public bool EnableLog { get; set; }

        public int InGameWidth { get; set; }
        public int InGameHeight { get; set; }

        public bool DebugCrashes { get; set; }
    }
}
