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

        public const string LogFilePath = "log/debug.log";
        public const string LogFileFmt = "log/{0}-debug.log";

        public const string SfxDirectory = "sfx";
        public const string MfxDirectory = "mfx";
        public const string JboxDirectory = "jbox";

        public const string FriendListFile = "config/friends.ini";
        public const string IgnoreListFile = "config/ignore.ini";

        public const string InventoryFile = "config/inventory.ini";
        public const string SpellsFile = "config/spells.ini";
        public const string PanelLayoutFile = "config/layout.ini";
        public const string ChatLogFile = "chatlog.txt";

        //Should be easily customizable between different clients (based on graphics)
        //not a config option because this shouldn't be exposed at the user level
        public static readonly int[] TrapSpikeGFXObjectIDs = { 449, 450, 451, 452 };

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
    }
}
