// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Chat;
using EndlessClient.Rendering;
using EOLib.Config;
using EOLib.Domain.Chat;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Services;
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
            //((EOClient) m_client).EventSendData +=
            //    dte => Logger.Log("SEND thread: Processing       {4} packet Family={0,-13} Action={1,-8} sz={2,-5} data={3}",
            //        Enum.GetName(typeof (PacketFamily), dte.PacketFamily),
            //        Enum.GetName(typeof (PacketAction), dte.PacketAction),
            //        dte.RawByteData.Length,
            //        dte.ByteDataHexString,
            //        dte.Type == DataTransferEventArgs.TransferType.Send
            //            ? "ENC"
            //            : dte.Type == DataTransferEventArgs.TransferType.SendRaw ? "RAW" : "ERR");
            //((EOClient) m_client).EventReceiveData +=
            //    dte => Logger.Log("RECV thread: Processing {0} packet Family={1,-13} Action={2,-8} sz={3,-5} data={4}",
            //        dte.PacketHandled ? "  handled" : "UNHANDLED",
            //        Enum.GetName(typeof (PacketFamily), dte.PacketFamily),
            //        Enum.GetName(typeof (PacketAction), dte.PacketAction),
            //        dte.RawByteData.Length,
            //        dte.ByteDataHexString);
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

        public IPubFile<EIFRecord> EIF { get; private set; }

        public IPubFile<ENFRecord> ENF { get; private set; }

        public IPubFile<ESFRecord> ESF { get; private set; }

        public IPubFile<ECFRecord> ECF { get; private set; }

        /// <summary>
        /// Stores a list of MapFiles paired with/accessible by their IDs
        /// </summary>
        private Dictionary<int, IMapFile> MapCache { get; set; }

        public Dictionary<DataFiles, EDFFile> DataFiles { get; private set; }

        private OldMapRenderer m_mapRender;
        /// <summary>
        /// Returns a map rendering object encapsulating the map the MainPlayer is on
        /// </summary>
        public OldMapRenderer ActiveMapRenderer
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

        /*** Functions for loading/checking the different pub/map files ***/

        //tries to load the map that MainPlayer.ActiveCharacter is hanging out on
        private bool _tryLoadMap(int mapID, bool forceReload)
        {
            return true;
        }

        public bool CheckMap(short mapID, byte[] mapRid, int mapFileSize)
        {
            NeedMap = -1;

            string mapFile = string.Format(EOLib.IO.Map.MapFile.MapFileFormatString, mapID);
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

            EOGame.Instance.Hud.AddChat(ChatTab.Local, GetString(EOResourceID.STRING_SERVER), GetString(EOResourceID.SERVER_MESSAGE_MAP_MUTATION), ChatIcon.Exclamation, ChatColor.Server);
            EOGame.Instance.Hud.AddChat(ChatTab.System, GetString(EOResourceID.STRING_SERVER), GetString(EOResourceID.SERVER_MESSAGE_MAP_MUTATION), ChatIcon.Exclamation, ChatColor.Server);

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
