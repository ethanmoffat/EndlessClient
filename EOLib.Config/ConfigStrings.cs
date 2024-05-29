using System.Diagnostics.CodeAnalysis;

namespace EOLib.Config
{
    [ExcludeFromCodeCoverage]
    public static class ConfigStrings
    {
        public const string Default_Config_File = "config/settings.ini";

        public const string Connection = "CONNECTION";
        public const string Host = "gameserver.ddns.eo-rs.dev";
        public const string Port = "8078";

        public const string Version = "VERSION";
        public const string Major = "Major";
        public const string Minor = "Minor";
        public const string Client = "Client";

        public const string Settings = "SETTINGS";
        public const string ShowShadows = "ShowShadows";
        public const string ShowTransition = "ShowTransition";
        public const string EnableLogging = "EnableLogging";
        public const string Music = "Music";
        public const string Sound = "Sound";
        public const string ShowBaloons = "ShowBaloons";

        public const string InGameWidth = nameof(InGameWidth);
        public static string InGameHeight = nameof(InGameHeight);

        public const string Custom = "CUSTOM";
        public const string NPCDropProtectTime = "NPCDropProtectTime";
        public const string PlayerDropProtectTime = "PlayerDropProtectTime";
        public const string MainCloneCompat = nameof(MainCloneCompat);
        public const string AccountCreateTimeout = nameof(AccountCreateTimeout);

        public const string LANGUAGE = "LANGUAGE";
        public const string Language = "Language";

        public const string Chat = "CHAT";
        public const string Filter = "Filter";
        public const string FilterAll = "FilterAll";
        public const string HearWhisper = "HearWhisper";
        public const string Interaction = "Interaction";
        public const string LogChat = "LogChat";
    }
}