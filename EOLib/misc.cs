using System;
using System.IO;
using System.Runtime.InteropServices;

namespace EOLib
{
    public static class Constants
    {
        public const int MaxChallenge = 11_092_110;

        public const int ResponseTimeout = 5000;
        public const int ResponseFileTimeout = 10000;

        public const byte ViewLength = 16;

        public const int LockerMaxSingleItemAmount = 200;
        public const int MaxLockerUpgrades = 7;
        public const int PartyRequestTimeoutSeconds = 15;
        public const int TradeRequestTimeoutSeconds = 15;
        public const int MuteDefaultTimeMinutes = 5;

        public const int GhostTime = 5;

        public static string SfxDirectory { get; } = GetPath("sfx");
        public static string MfxDirectory { get; } = GetPath("mfx");
        public static string JboxDirectory { get; } = GetPath("jbox");

        public static string FriendListFile { get; } = GetModifiablePath("config/friends.ini");
        public static string IgnoreListFile { get; } = GetModifiablePath("config/ignore.ini");

        public static string InventoryFile { get; } = GetModifiablePath("config/inventory.ini");
        public static string SpellsFile { get; } = GetModifiablePath("config/spells.ini");
        public static string PanelLayoutFile { get; } = GetModifiablePath("config/layout.ini");
        public static string ChatLogFile { get; } = GetModifiablePath("chatlog.txt");

        //Should be easily customizable between different clients (based on graphics)
        //not a config option because this shouldn't be exposed at the user level
        public static readonly int[] TrapSpikeGFXObjectIDs = { 449, 450, 451, 452 };

        //not a config option because this shouldn't be exposed at the user level
        public const int NPCDropProtectSeconds = 30;
        public const int PlayerDropProtectSeconds = 5;

        // Weapon graphics of instruments (there is no pub flag for this)
        public static readonly int[] Instruments = { 49, 50 };
        public const string FontSize07 = @"BitmapFonts/sans_09px";
        public const string FontSize08 = @"BitmapFonts/sans_11px";
        public const string FontSize08pt5 = @"BitmapFonts/sans_11px_103pct";
        public const string FontSize09 = @"BitmapFonts/sans_12px";
        public const string FontSize10 = @"BitmapFonts/sans_13px";

        public const int OutOfBand_Packets_Handled_Per_Update = 10;

        public const string CreditsText = @"Endless Online - C# Client
Developed by Ethan Moffat
Based on Endless Online --
Copyright Vult-R

Thanks to :
--Sausage for eoserv + C# EO libs
--eoserv.net community
--Hotdog for Eodev client

Contributors : 
-- Sorokya
-- Septharoth
-- miou-gh
-- CoderDanUK";

        public const string VersionInfoFormat = "{0}.{1:000}.{2:000} - {3}:{4}";

        private static string GetPath(string inputPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine("Contents", "Resources", inputPath);
            }
            else
            {
                return inputPath;
            }
        }

        private static string GetModifiablePath(string inputPath)
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
