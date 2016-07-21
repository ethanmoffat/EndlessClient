// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Chat;
using EOLib.Domain.Map;
using EOLib.Domain.Protocol;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Old;
using EOLib.IO.OldMap;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.API;
using EOLib.Net.PacketProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient
{
    [Serializable]
    public class WorldLoadException : Exception
    {
        public WorldLoadException(string msg) : base(msg) { }
    }

    //singleton pattern: provides global access to data files and network connection
    //    without allowing for instantiation outside of the class or inheriting from it
    public sealed class OldWorld : IDisposable
    {
#if DEBUG
        public static int FPS { get; set; }
#endif
        /*** STATIC MEMBERS AND SUCH FOR THE SINGLETON PATTERN ***/
        private static OldWorld inst;
        private static readonly object locker = new object();

        public static bool Initialized { get; private set; }

        public static OldWorld Instance
        {
            get
            {
                lock (locker)
                {
                    return inst ?? (inst = new OldWorld());
                }
            }
        }

        //Gets a localized string based on the selected language
        public static string GetString(DialogResourceID id, bool getTitle)
        {
            return Instance.DataFiles[Instance.Localized1].Data[(int)id + (getTitle ? 0 : 1)];
        }
        public static string GetString(EOResourceID id)
        {
            return Instance.DataFiles[Instance.Localized2].Data[(int)id];
        }

        private OldWorld() //private: don't allow construction of the world using 'new'
        {
            _tryLoadItems();
            _tryLoadNPCs();
            _tryLoadSpells();
            _tryLoadClasses();
            
            //initial capacity of 32: most players won't travel between too many maps in a gaming session
            MapCache = new Dictionary<int, MapFile>(32);
            DataFiles = new Dictionary<DataFiles, EDFFile>(12); //12 files total
            m_player = new Player();

            //client construction: logging for when packets are sent/received
            m_client = new EOClient(CreatePacketProcessorActions());
            ((EOClient) m_client).EventSendData +=
                dte => Logger.Log("SEND thread: Processing       {4} packet Family={0,-13} Action={1,-8} sz={2,-5} data={3}",
                    Enum.GetName(typeof (PacketFamily), dte.PacketFamily),
                    Enum.GetName(typeof (PacketAction), dte.PacketAction),
                    dte.RawByteData.Length,
                    dte.ByteDataHexString,
                    dte.Type == DataTransferEventArgs.TransferType.Send
                        ? "ENC"
                        : dte.Type == DataTransferEventArgs.TransferType.SendRaw ? "RAW" : "ERR");
            ((EOClient) m_client).EventReceiveData +=
                dte => Logger.Log("RECV thread: Processing {0} packet Family={1,-13} Action={2,-8} sz={3,-5} data={4}",
                    dte.PacketHandled ? "  handled" : "UNHANDLED",
                    Enum.GetName(typeof (PacketFamily), dte.PacketFamily),
                    Enum.GetName(typeof (PacketAction), dte.PacketAction),
                    dte.RawByteData.Length,
                    dte.ByteDataHexString);

            ((EOClient) m_client).EventDisconnect += () =>
            {
                EOGame.Instance.ShowLostConnectionDialog();
                EOGame.Instance.ResetWorldElements();
                EOGame.Instance.SetInitialGameState();
            };
        }

        private static IPacketProcessorActions CreatePacketProcessorActions()
        {
            return new PacketProcessActions(new SequenceRepository(),
                                            new PacketEncoderRepository(),
                                            new PacketEncoderService(),
                                            new PacketSequenceService());
        }

        public void Init()
        {
            exp_table = new int[254];
            for (int i = 1; i < exp_table.Length; ++i)
            {
                exp_table[i] = (int)Math.Round(Math.Pow(i, 3) * 133.1);
            }
        }

        public int[] exp_table;

        private EOLanguage m_lang;
        public EOLanguage Language
        {
            get { return m_lang; }
            set
            {
                m_lang = value;
                switch (m_lang)
                {
                    case EOLanguage.English:
                        Localized1 = EOLib.Localization.DataFiles.EnglishStatus1;
                        Localized2 = EOLib.Localization.DataFiles.EnglishStatus2;
                        break;
                    case EOLanguage.Dutch:
                        Localized1 = EOLib.Localization.DataFiles.DutchStatus1;
                        Localized2 = EOLib.Localization.DataFiles.DutchStatus2;
                        break;
                    case EOLanguage.Swedish:
                        Localized1 = EOLib.Localization.DataFiles.SwedishStatus1;
                        Localized2 = EOLib.Localization.DataFiles.SwedishStatus2;
                        break;
                    case EOLanguage.Portuguese:
                        Localized1 = EOLib.Localization.DataFiles.PortugueseStatus1;
                        Localized2 = EOLib.Localization.DataFiles.PortugueseStatus2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public DataFiles Localized1 { get; private set; }
        public DataFiles Localized2 { get; private set; }

        public bool CurseFilterEnabled { get; set; }
        public bool StrictFilterEnabled { get; set; }

        public bool ShowShadows { get; set; }
        public bool ShowChatBubbles { get; set; }
        public bool ShowTransition { get; private set; }
        public int PlayerDropProtectTime { get; private set; }
        public int NPCDropProtectTime { get; private set; }

        public bool MusicEnabled { get; set; }
        public bool SoundEnabled { get; set; }

        public bool HearWhispers { get; set; }
        public bool Interaction { get; set; }
        public bool LogChatToFile { get; set; }

        public bool EnableLog { get; private set; }

        /*** Instance Properties and such ***/

        public short JailMap { get; private set; }

        //this is an int for the map id since there are multiple maps
        public int NeedMap { get; private set; }
        public bool NeedEIF { get; private set; }
        public bool NeedENF { get; private set; }
        public bool NeedESF { get; private set; }
        public bool NeedECF { get; private set; }

        public IDataFile<ItemRecord> EIF { get; private set; }

        public IDataFile<NPCRecord> ENF { get; private set; }

        public IDataFile<SpellRecord> ESF { get; private set; }

        public IDataFile<ClassRecord> ECF { get; private set; }

        /// <summary>
        /// Stores a list of MapFiles paired with/accessible by their IDs
        /// </summary>
        private Dictionary<int, MapFile> MapCache { get; set; }

        public Dictionary<DataFiles, EDFFile> DataFiles { get; private set; }

        private MapRenderer m_mapRender;
        /// <summary>
        /// Returns a map rendering object encapsulating the map the MainPlayer is on
        /// </summary>
        public MapRenderer ActiveMapRenderer
        {
            get
            {
                //make sure it's in the game's componenets
                if(m_mapRender != null && EOGame.Instance.State == GameStates.PlayingTheGame && !EOGame.Instance.Components.Contains(m_mapRender))
                    EOGame.Instance.Components.Add(m_mapRender);

                return m_mapRender;
            }
        }

        private OldCharacterRenderer m_charRender;

        /// <summary>
        /// Returns a reference to the primary CharacterRenderer associated with MainPlayer.ActiveCharacter
        /// </summary>
        public OldCharacterRenderer ActiveCharacterRenderer
        {
            get
            {
                //lazy initialization
                if (m_charRender == null)
                {
                    m_charRender = new OldCharacterRenderer(MainPlayer.ActiveCharacter);
                    m_charRender.Initialize();
                }

                OldCharacterRenderer ret = m_charRender;

                if(ret.Character == null)
                {
                    EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
                    return null;
                }

                //if player logs out and logs back in
                if (ret.Character != MainPlayer.ActiveCharacter)
                {
                    ret.Dispose();
                    ret = m_charRender = new OldCharacterRenderer(MainPlayer.ActiveCharacter);
                    m_charRender.Initialize();
                }

                return ret;
            }
        }

        /*** Other things that should be singleton ***/

        private readonly Player m_player;
        public Player MainPlayer
        {
            get { return m_player; }
        }
        
        private readonly ClientBase m_client;
        public ClientBase Client
        {
            get { return m_client; }
        }

        private PacketAPI m_api;
        public void SetAPIHandle(PacketAPI api) { m_api = api; }

        /*** Functions for loading/checking the different pub/map files ***/

        //tries to load the map that MainPlayer.ActiveCharacter is hanging out on
        private bool _tryLoadMap(int mapID, bool forceReload)
        {
            try
            {
                if (mapID < 0)
                    mapID = MainPlayer.ActiveCharacter.CurrentMap;

                string mapFile = Path.Combine("maps", string.Format("{0,5:D5}.emf", mapID));

                if (!MapCache.ContainsKey(mapID))
                {
                    MapCache.Add(mapID, new MapFile());
                    MapCache[mapID].Load(mapFile);
                }
                else if (forceReload)
                    MapCache[mapID].Load(mapFile);


                //map renderer construction moved to be more closely coupled to loading of the map
                if (m_mapRender == null)
                    m_mapRender = new MapRenderer(EOGame.Instance, m_api);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool _tryLoadItems(string fileName = null)
        {
            EIF = new ItemFile();
            try
            {
                EIF.Load(fileName);
            }
            catch
            {
                EIF = null;
                return false;
            }

            return true;
        }

        private bool _tryLoadNPCs(string fileName = null)
        {
            ENF = new NPCFile();
            try
            {
                ENF.Load(fileName);
            }
            catch
            {
                ENF = null;
                return false;
            }

            return true;
        }

        private bool _tryLoadSpells(string fileName = null)
        {
            ESF = new SpellFile();
            try
            {
                ESF.Load(fileName);
            }
            catch
            {
                ESF = null;
                return false;
            }

            return true;
        }

        private bool _tryLoadClasses(string fileName = null)
        {
            ECF = new ClassFile();
            try
            {
                ECF.Load(fileName);
            }
            catch
            {
                ECF = null;
                return false;
            }

            return true;
        }

        public bool CheckMap(short mapID, byte[] mapRid, int mapFileSize)
        {
            NeedMap = -1;

            string mapFile = string.Format(Constants.MapFileFormatString, mapID);
            if (!Directory.Exists("maps") || !File.Exists(mapFile))
            {
                Directory.CreateDirectory("maps");
                NeedMap = mapID;
                return false;
            }

            //try to load the map if it isn't cached. on failure, set needmap
            if (!MapCache.ContainsKey(mapID))
                NeedMap = _tryLoadMap(mapID, true) ? -1 : mapID;

            //on success of file load, check the rid and the size of the file
            if (MapCache.ContainsKey(mapID))
            {
                for (int i = 0; i < 4; ++i)
                {
                    if(MapCache[mapID].Properties.Checksum[i] != mapRid[i])
                    {
                        NeedMap = mapID;
                        break;
                    }
                }

                if (NeedMap == -1 && MapCache[mapID].Properties.FileSize != mapFileSize)
                    NeedMap = mapID;
            }

            //return true if the map is not needed
            return NeedMap == -1;
        }

        private void _checkPub(InitFileType file, int rid, short len)
        {
            const string fName = "pub\\";
            if (!Directory.Exists(fName))
                Directory.CreateDirectory(fName);

            switch (file)
            {
                case InitFileType.Item:
                    NeedEIF = !_tryLoadItems();
                    if(EIF != null)
                        NeedEIF = rid != EIF.Rid || len != EIF.Len;
                    break;
                case InitFileType.Npc:
                    NeedENF = !_tryLoadNPCs();
                    if (ENF != null)
                        NeedENF = rid != ENF.Rid || len != ENF.Len;
                    break;
                case InitFileType.Spell:
                    NeedESF = !_tryLoadSpells();
                    if (ESF != null)
                        NeedESF = rid != ESF.Rid || len != ESF.Len;
                    break;
                case InitFileType.Class:
                    NeedECF = !_tryLoadClasses();
                    if (ECF != null)
                        NeedECF = rid != ECF.Rid || len != ECF.Len;
                    break;
                default:
                    return;
            }
        }

        public void WarpAgreeAction(short mapID, WarpAnimation anim, List<CharacterData> chars, List<NPCData> npcs, List<OldMapItem> items)
        {
            if (!_tryLoadMap(mapID, false))
            {
                EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
                return;
            }

            if(mapID > 0)
                MainPlayer.ActiveCharacter.CurrentMap = mapID;

            if(ActiveMapRenderer.MapRef != MapCache[MainPlayer.ActiveCharacter.CurrentMap])
                ActiveMapRenderer.SetActiveMap(MapCache[MainPlayer.ActiveCharacter.CurrentMap]);

            ActiveMapRenderer.ClearOtherPlayers();
            ActiveMapRenderer.ClearOtherNPCs();
            ActiveMapRenderer.ClearMapItems();

            foreach (var data in chars)
            {
                if (data.ID == MainPlayer.ActiveCharacter.ID)
                    MainPlayer.ActiveCharacter.ApplyData(data);
                else
                    ActiveMapRenderer.AddOtherPlayer(data);
            }

            foreach (var data in npcs)
                ActiveMapRenderer.AddOtherNPC(data);

            foreach (OldMapItem mi in items)
                ActiveMapRenderer.AddMapItem(mi);

            if (anim == WarpAnimation.Admin)
                ActiveCharacterRenderer.ShowWarpArrive();
        }

        public void ApplyWelcomeRequest(PacketAPI api, WelcomeRequestData data)
        {
            MainPlayer.SetPlayerID(data.PlayerID);
            MainPlayer.SetActiveCharacter(api, data.ActiveCharacterID);
            MainPlayer.ActiveCharacter.CurrentMap = data.MapID;

            CheckMap(data.MapID, data.MapRID, data.MapLen);
            _checkPub(InitFileType.Item, data.EifRid, data.EifLen);
            _checkPub(InitFileType.Npc, data.EnfRid, data.EnfLen);
            _checkPub(InitFileType.Spell, data.EsfRid, data.EsfLen);
            _checkPub(InitFileType.Class, data.EcfRid, data.EcfLen);

            MainPlayer.ActiveCharacter.Name = data.Name;
            MainPlayer.ActiveCharacter.Title = data.Title;
            MainPlayer.ActiveCharacter.GuildName = data.GuildName;
            MainPlayer.ActiveCharacter.GuildRankStr = data.GuildRankStr;
            MainPlayer.ActiveCharacter.GuildRankNum = data.GuildRankNum;
            MainPlayer.ActiveCharacter.Class = data.ClassID;
            MainPlayer.ActiveCharacter.PaddedGuildTag = data.PaddedGuildTag;
            MainPlayer.ActiveCharacter.AdminLevel = data.AdminLevel;

            MainPlayer.ActiveCharacter.Stats = new CharStatData
            {
                Level = data.Level,
                Experience = data.Exp,
                Usage = data.Usage,

                HP = data.HP,
                MaxHP = data.MaxHP,
                TP = data.TP,
                MaxTP = data.MaxTP,
                SP = data.MaxSP,
                MaxSP = data.MaxSP,

                StatPoints = data.StatPoints,
                SkillPoints = data.SkillPoints,
                MinDam = data.MinDam,
                MaxDam = data.MaxDam,
                Karma = data.Karma,
                Accuracy = data.Accuracy,
                Evade = data.Evade,
                Armor = data.Armor,
                Str = data.DispStr,
                Int = data.DispInt,
                Wis = data.DispWis,
                Agi = data.DispAgi,
                Con = data.DispCon,
                Cha = data.DispCha
            };

            Array.Copy(data.PaperDoll, MainPlayer.ActiveCharacter.PaperDoll, (int) EquipLocation.PAPERDOLL_MAX);
            JailMap = data.JailMap;
        }

        public void ApplyWelcomeMessage(WelcomeMessageData data)
        {
            MainPlayer.ActiveCharacter.Weight = data.Weight;
            MainPlayer.ActiveCharacter.MaxWeight = data.MaxWeight;

            MainPlayer.ActiveCharacter.Inventory.Clear();
            MainPlayer.ActiveCharacter.Inventory.AddRange(data.Inventory);
            MainPlayer.ActiveCharacter.Spells.Clear();
            MainPlayer.ActiveCharacter.Spells.AddRange(data.Spells);

            if (ActiveMapRenderer.MapRef == null)
                ActiveMapRenderer.SetActiveMap(MapCache[MainPlayer.ActiveCharacter.CurrentMap]);

            ActiveMapRenderer.ClearOtherPlayers();
            ActiveMapRenderer.ClearOtherNPCs();
            ActiveMapRenderer.ClearMapItems();

            var characterList = data.CharacterData.ToList();
            var mainCharacter = characterList.Find(x => x.Name.ToLower() == MainPlayer.ActiveCharacter.Name.ToLower());
            MainPlayer.ActiveCharacter.ApplyData(mainCharacter, false); //do NOT copy paperdoll data over the existing!
            characterList.Remove(mainCharacter);

            foreach (var character in characterList)
                ActiveMapRenderer.AddOtherPlayer(character);
            foreach (var npc in data.NPCData)
                ActiveMapRenderer.AddOtherNPC(npc);
            foreach (var item in data.MapItemData)
                ActiveMapRenderer.AddMapItem(item);
        }

        public void ResetGameElements()
        {
            if (m_mapRender != null)
            {
                m_mapRender.Dispose();
                m_mapRender = null;
            }

            if (m_charRender != null)
            {
                m_charRender.Dispose();
                m_charRender = null;
            }

            if(MapCache != null) MapCache.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_mapRender != null)
                    m_mapRender.Dispose();

                if (m_charRender != null)
                    m_charRender.Dispose();

                if (m_client != null)
                    m_client.Dispose();
            }
        }

        public void Remap()
        {
            MapCache.Remove(MainPlayer.ActiveCharacter.CurrentMap);
            if (!_tryLoadMap(-1, true))
            {
                EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
                return;
            }

            EOGame.Instance.Hud.AddChat(ChatTabs.Local, GetString(EOResourceID.STRING_SERVER), GetString(EOResourceID.SERVER_MESSAGE_MAP_MUTATION), ChatType.Exclamation, ChatColor.Server);
            EOGame.Instance.Hud.AddChat(ChatTabs.System, GetString(EOResourceID.STRING_SERVER), GetString(EOResourceID.SERVER_MESSAGE_MAP_MUTATION), ChatType.Exclamation, ChatColor.Server);

            ActiveMapRenderer.SetActiveMap(MapCache[MainPlayer.ActiveCharacter.CurrentMap]);
        }

        public static void IgnoreDialogs(XNAControl control)
        {
            control.IgnoreDialog(typeof(EOPaperdollDialog));
            control.IgnoreDialog(typeof(ChestDialog));
            control.IgnoreDialog(typeof(ShopDialog));
            control.IgnoreDialog(typeof(BankAccountDialog));
            control.IgnoreDialog(typeof(LockerDialog));
            control.IgnoreDialog(typeof(TradeDialog));
            control.IgnoreDialog(typeof(FriendIgnoreListDialog));
            control.IgnoreDialog(typeof(SkillmasterDialog));
            control.IgnoreDialog(typeof(QuestDialog));
            control.IgnoreDialog(typeof(QuestProgressDialog));
        }

        public static Texture2D GetSpellIcon(short icon, bool hover)
        {
            Texture2D fullTexture = EOGame.Instance.GFXManager.TextureFromResource(GFXTypes.SpellIcons, icon);
            Texture2D ret = new Texture2D(fullTexture.GraphicsDevice, fullTexture.Width / 2, fullTexture.Height);

            Color[] data = new Color[ret.Width * ret.Height];
            fullTexture.GetData(0, new Rectangle(hover ? ret.Width : 0, 0, ret.Width, ret.Height), data, 0, data.Length);
            ret.SetData(data);

            return ret;
        }
    }
}
