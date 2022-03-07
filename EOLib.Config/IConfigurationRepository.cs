using AutomaticTypeMapper;

namespace EOLib.Config
{
    public interface IConfigurationRepository
    {
        string Host { get; set; }
        int Port { get; set; }

        byte VersionMajor { get; set; }
        byte VersionMinor { get; set; }
        byte VersionBuild { get; set; }

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

        bool MainCloneCompat { get; set; }

        bool EnableLog { get; set; }
    }

    public interface IConfigurationProvider
    {
        string Host { get; }
        int Port { get; }

        byte VersionMajor { get; }
        byte VersionMinor { get; }
        byte VersionBuild { get; }

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

        bool MainCloneCompat { get; }

        bool EnableLog { get; }
    }

    [MappedType(BaseType = typeof(IConfigurationRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IConfigurationProvider), IsSingleton = true)]
    public class ConfigurationRepository : IConfigurationRepository, IConfigurationProvider
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public byte VersionBuild { get; set; }

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

        public bool MainCloneCompat { get; set; }

        public bool EnableLog { get; set; }
    }
}
