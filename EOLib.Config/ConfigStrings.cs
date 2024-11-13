using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace EOLib.Config
{
    [ExcludeFromCodeCoverage]
    public static class ConfigStrings
    {
        public static string Default_Config_File { get; } = GetPath("config/settings.ini");

        public const string Connection = "CONNECTION";
        public const string Host = "Host";
        public const string Port = "Port";

        public const string Version = "VERSION";
        public const string Major = "Major";
        public const string Minor = "Minor";
        public const string Client = "Client";

        public const string Settings = "SETTINGS";
        public const string ShowShadows = "ShowShadows";
        public const string ShowTransition = "ShowTransition";
        public const string Music = "Music";
        public const string Sound = "Sound";
        public const string ShowBaloons = "ShowBaloons";

        public const string InGameWidth = nameof(InGameWidth);
        public static string InGameHeight = nameof(InGameHeight);

        public const string Custom = "CUSTOM";
        public const string AccountCreateTimeout = nameof(AccountCreateTimeout);

        public const string LANGUAGE = "LANGUAGE";
        public const string Language = "Language";

        public const string Chat = "CHAT";
        public const string Filter = "Filter";
        public const string FilterAll = "FilterAll";
        public const string HearWhisper = "HearWhisper";
        public const string Interaction = "Interaction";
        public const string LogChat = "LogChat";

        private static string GetPath(string inputPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                return Path.Combine(home, ".endlessclient", inputPath);
            }
            else
            {
                return inputPath;
            }
        }
    }
}
