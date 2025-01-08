namespace EOLib.Shared
{
    public static class Constants
    {
        public const int MaxChallenge = 11_092_110;

        public const byte ViewLength = 16;

        public const int LockerMaxSingleItemAmount = 200;
        public const int MaxLockerUpgrades = 7;
        public const int PartyRequestTimeoutSeconds = 15;
        public const int TradeRequestTimeoutSeconds = 15;
        public const int MuteDefaultTimeMinutes = 5;

        public const int GhostTime = 5;

        public const int ExpectedNumberOfDataFiles = 12;

        public static string Default_Config_File { get; } = PathResolver.GetModifiablePath("config/settings.ini");

        public static string DataFilePath { get; } = PathResolver.GetPath("data");
        public static string DataFileFormat { get; } = PathResolver.GetPath("data/dat{0,3:D3}.edf");

        public static string MapDirectory { get; } = PathResolver.GetModifiablePath("maps");
        public static string MapFileFormatString { get; } = $"{MapDirectory}/{{0,5:D5}}.emf";

        public static string PubDirectory = "pub";
        public static string EIFFormat { get; } = PathResolver.GetModifiablePath("pub/dat{0,3:D3}.eif");
        public static string ENFFormat { get; } = PathResolver.GetModifiablePath("pub/dtn{0,3:D3}.enf");
        public static string ESFFormat { get; } = PathResolver.GetModifiablePath("pub/dsl{0,3:D3}.esf");
        public static string ECFFormat { get; } = PathResolver.GetModifiablePath("pub/dat{0,3:D3}.ecf");

        public const string EIFFilter = "dat*.eif";
        public const string ENFFilter = "dtn*.enf";
        public const string ESFFilter = "dsl*.esf";
        public const string ECFFilter = "dat*.ecf";

        public static string GFXFormat { get; } = PathResolver.GetPath("gfx/gfx{0,3:D3}.egf");

        public static string SfxDirectory { get; } = PathResolver.GetPath("sfx");
        public static string MfxDirectory { get; } = PathResolver.GetPath("mfx");
        public static string JboxDirectory { get; } = PathResolver.GetPath("jbox");

        public static string FriendListFile { get; } = PathResolver.GetModifiablePath("config/friends.ini");
        public static string IgnoreListFile { get; } = PathResolver.GetModifiablePath("config/ignore.ini");

        public static string InventoryFile { get; } = PathResolver.GetModifiablePath("config/inventory.ini");
        public static string SpellsFile { get; } = PathResolver.GetModifiablePath("config/spells.ini");
        public static string PanelLayoutFile { get; } = PathResolver.GetModifiablePath("config/layout.ini");
        public static string ChatLogFile { get; } = PathResolver.GetModifiablePath("chatlog.txt");

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
-- MrDanOak
-- sjbmcg";

        public const string VersionInfoFormat = "{0}.{1:000}.{2:000} - {3}:{4}";
    }
}
