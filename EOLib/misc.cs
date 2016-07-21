// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib
{
    public static class ArrayExtension
    {
        public static T[] SubArray<T>(this T[] arr, int offset, int count)
        {
            T[] ret = new T[count];

            if (count == 1)
                ret[0] = arr[offset];

            for (int i = offset; i < offset + count; ++i)
                ret[i - offset] = arr[i];

            return ret;
        }
    }

    public static class DateTimeExtension
    {
        public static int ToEOTimeStamp(this DateTime dt)
        {
            return dt.Hour * 360000 + dt.Minute * 6000 + dt.Second * 100 + dt.Millisecond / 10;
        }
    }

    public static class Constants
    {
        public const int ChatBubbleTimeout = 4000;
        public const int ResponseTimeout = 5000;
        public const int ResponseFileTimeout = 10000;

        public const byte MajorVersion = 0;
        public const byte MinorVersion = 0;
        public const byte ClientVersion = 28;

        public const string Host = "127.0.0.1";
        public const int Port = 8078;

        public const string MapFileFormatString = "maps/{0,5:D5}.emf";

        public const string DataFilePath = "data/";
        public const int ExpectedNumberOfDataFiles = 12;

        public const byte ViewLength = 16;

        public const int NPCDropProtectionSeconds = 30;
        public const int PlayerDropProtectionSeconds = 5;
        public const int LockerMaxSingleItemAmount = 200;
        public const int PartyRequestTimeoutSeconds = 15;
        public const int TradeRequestTimeoutSeconds = 15;
        public const int MuteDefaultTimeMinutes = 5;

        public const string LogFilePath = "log/debug.log";
        public const string LogFileFmt = "log/{0}-debug.log";

        //Should be easily customizable between different clients (based on graphics)
        //not a config option because this shouldn't be exposed at the user level
        public static readonly int[] TrapSpikeGFXObjectIDs = {449, 450, 451, 452};

        public const string FontSize07 = @"Fonts/InGame_Main_07";
        public const string FontSize08 = @"Fonts/InGame_Main_08";
        public const string FontSize08pt5 = @"Fonts/InGame_Main_08pt5";
        public const string FontSize08pt75 = @"Fonts/InGame_Main_08pt75";
        public const string FontSize10 = @"Fonts/InGame_Main_10";

        public const int OutOfBand_Packets_Handled_Per_Update = 10;

        public const string CreditsText = @"Endless Online - C# Client
Developed by Ethan Moffat
Based on Endless Online --
Copyright Vult-R

Thanks to :
--Sausage for eoserv + C# EO libs
--eoserv.net community
--Hotdog for Eodev client";
        public const string VersionInfoFormat = "{0}.{1:000}.{2:000} - {3}:{4}";
    }

    public static class ConfigStrings
    {
        public const string Default_Config_File = "config/settings.ini";

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
        public const string EnableLogging = "EnableLogging";
        public const string Music = "Music";
        public const string Sound = "Sound";
        public const string ShowBaloons = "ShowBaloons";

        public const string Custom = "CUSTOM";
        public const string NPCDropProtectTime = "NPCDropProtectTime";
        public const string PlayerDropProtectTime = "PlayerDropProtectTime";

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
