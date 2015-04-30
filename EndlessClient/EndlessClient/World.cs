using System;
using System.Collections.Generic;
using System.IO;
using EndlessClient.Handlers;
using EOLib;
using EOLib.Data;
using EOLib.Net;

namespace EndlessClient
{
	[Serializable]
	public class WorldLoadException : Exception
	{
		public WorldLoadException(string msg) : base(msg) { }
	}

	//singleton pattern: provides global access to data files and network connection
	//	without allowing for instantiation outside of the class or inheriting from it
	public sealed class World : IDisposable
	{
#if DEBUG
		public static int FPS { get; set; }
#endif
		/*** STATIC MEMBERS AND SUCH FOR THE SINGLETON PATTERN ***/
		private static World inst;
		private static readonly object locker = new object();

		public static bool Initialized { get; private set; }

		public static World Instance
		{
			get
			{
				lock (locker)
				{
					return inst ?? (inst = new World());
				}
			}
		}

		//Gets a localized string based on the selected language
		//public static string GetString(DATCONST1 id)
		//{
		//	return Instance.DataFiles[Instance.Localized1].Data[(int) id];
		//}
		public static string GetString(DATCONST2 id)
		{
			return Instance.DataFiles[Instance.Localized2].Data[(int)id];
		}

		private World() //private: don't allow construction of the world using 'new'
		{
			_tryLoadItems();
			_tryLoadNPCs();
			_tryLoadSpells();
			_tryLoadClasses();
			
			//initial capacity of 32: most players won't travel between too many maps in a gaming session
			MapCache = new Dictionary<int, MapFile>(32);
			DataFiles = new Dictionary<DataFiles, EDFFile>(12); //12 files total
			m_player = new Player();
			m_config = new IniReader(@"config\settings.ini");
			if (!m_config.Load())
				throw new WorldLoadException("Unable to load the configuration file!");

			//client construction: logging for when packets are sent/received
			m_client = new EOClient(_getPacketHandlerDictionary());
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
			((EOClient) m_client).EventDisconnect += () => MainPlayer.Logout();
		}

		public void Init()
		{
			Initialized = true;

			exp_table = new int[254];
			for (int i = 1; i < exp_table.Length; ++i)
			{
				exp_table[i] = (int)Math.Round(Math.Pow(i, 3) * 133.1);
			}

			string[] files;
			if (!Directory.Exists(Constants.DataFilePath) || (files = Directory.GetFiles(Constants.DataFilePath, "*.edf")).Length != 12)
				throw new WorldLoadException("Unable to find data files! Check that the data directory exists and has ALL the edf files copied over");

			for (DataFiles file = (DataFiles)1; file <= (DataFiles)12; ++file)
				DataFiles.Add(file, new EDFFile(files[(int)file - 1], file));

			int maj, min, cli, lang, dropProtect;
			VersionMajor = m_config.GetValue(ConfigStrings.Version, ConfigStrings.Major, out maj) ? (byte)maj : Constants.MajorVersion;
			VersionMinor = m_config.GetValue(ConfigStrings.Version, ConfigStrings.Minor, out min) ? (byte)min : Constants.MinorVersion;
			VersionClient = m_config.GetValue(ConfigStrings.Version, ConfigStrings.Client, out cli) ? (byte)cli : Constants.ClientVersion;
			Language = m_config.GetValue(ConfigStrings.LANGUAGE, ConfigStrings.Language, out lang) ? (EOLanguage)lang : EOLanguage.English;

			PlayerDropProtectTime = m_config.GetValue(ConfigStrings.Custom, ConfigStrings.PlayerDropProtectTime, out dropProtect)
				? dropProtect
				: Constants.PlayerDropProtectionSeconds;
			NPCDropProtectTime = m_config.GetValue(ConfigStrings.Custom, ConfigStrings.NPCDropProtectTime, out dropProtect)
				? dropProtect
				: Constants.NPCDropProtectionSeconds;
			
			bool filter, strict;
			CurseFilterEnabled = m_config.GetValue(ConfigStrings.Chat, ConfigStrings.Filter, out filter) && filter;
			StrictFilterEnabled = m_config.GetValue(ConfigStrings.Chat, ConfigStrings.FilterAll, out strict) && strict;

			bool shadows, transitions, music, sound, bubbles;
			ShowShadows = !m_config.GetValue(ConfigStrings.Settings, ConfigStrings.ShowShadows, out shadows) || shadows;
			ShowTransition = !m_config.GetValue(ConfigStrings.Settings, ConfigStrings.ShowTransition, out transitions) || transitions;
			MusicEnabled = m_config.GetValue(ConfigStrings.Settings, ConfigStrings.Music, out music) && music;
			SoundEnabled = m_config.GetValue(ConfigStrings.Settings, ConfigStrings.Sound, out sound) && sound;
			ShowChatBubbles = !m_config.GetValue(ConfigStrings.Settings, ConfigStrings.ShowBaloons, out bubbles) || bubbles;

			bool logging, whispers, interaction, logChat;
			EnableLog = m_config.GetValue(ConfigStrings.Settings, ConfigStrings.EnableLogging, out logging) && logging;
			HearWhispers = !m_config.GetValue(ConfigStrings.Chat, ConfigStrings.HearWhisper, out whispers) || whispers;
			Interaction = !m_config.GetValue(ConfigStrings.Chat, ConfigStrings.Interaction, out interaction) || interaction;
			LogChatToFile = m_config.GetValue(ConfigStrings.Chat, ConfigStrings.LogChat, out logChat) && logChat;

			//do these last and throw non-fatal exceptions (default will be used)
			string host;
			int port;
			if (!m_config.GetValue(ConfigStrings.Connection, ConfigStrings.Host, out host))
			{
				Host = Constants.Host;
				throw new ConfigStringLoadException(ConfigStrings.Host);
			}
			Host = host;
			if (!m_config.GetValue(ConfigStrings.Connection, ConfigStrings.Port, out port))
			{
				Port = Constants.Port;
				throw new ConfigStringLoadException(ConfigStrings.Port);
			}
			Port = port;
		}

		public int[] exp_table;

		/*** Settings loaded from configuration ***/
		/*** Those with private setters are custom config options that can't be changed in-game ***/
		private readonly IniReader m_config;

		public byte VersionMajor { get; private set; }
		public byte VersionMinor { get; private set; }
		public byte VersionClient { get; private set; }

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
						Localized1 = EOLib.DataFiles.EnglishStatus1;
						Localized2 = EOLib.DataFiles.EnglishStatus2;
						break;
					case EOLanguage.Dutch:
						Localized1 = EOLib.DataFiles.DutchStatus1;
						Localized2 = EOLib.DataFiles.DutchStatus2;
						break;
					case EOLanguage.Swedish:
						Localized1 = EOLib.DataFiles.SwedishStatus1;
						Localized2 = EOLib.DataFiles.SwedishStatus2;
						break;
					case EOLanguage.Portuguese:
						Localized1 = EOLib.DataFiles.PortugueseStatus1;
						Localized2 = EOLib.DataFiles.PortugueseStatus2;
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

		public string Host { get; private set; }
		public int Port { get; private set; }

		public bool HearWhispers { get; set; }
		public bool Interaction { get; set; }
		public bool LogChatToFile { get; set; }

		public bool EnableLog { get; private set; }

		/*** Instance Properties and such ***/

// ReSharper disable UnusedAutoPropertyAccessor.Global
		public short JailMap { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

		//this is an int for the map id since there are multiple maps
		public int NeedMap { get; private set; }
		public bool NeedEIF { get; private set; }
		public bool NeedENF { get; private set; }
		public bool NeedESF { get; private set; }
		public bool NeedECF { get; private set; }

		private ItemFile m_items;
		public ItemFile EIF
		{
			get { return m_items; }
		}

		private NPCFile m_npcs;
		public NPCFile ENF
		{
			get { return m_npcs; }
		}

		private SpellFile m_spells;
// ReSharper disable MemberCanBePrivate.Global
		public SpellFile ESF
// ReSharper restore MemberCanBePrivate.Global
		{
			get { return m_spells; }
		}

		private ClassFile m_classes;
		public ClassFile ECF
		{
			get { return m_classes; }
		}

		/// <summary>
		/// Stores a list of MapFiles paired with/accessible by their IDs
		/// </summary>
		private Dictionary<int, MapFile> MapCache { get; set; }

		public Dictionary<DataFiles, EDFFile> DataFiles { get; private set; }

		/// <summary>
		/// Returns a MapFile for the map the MainPlayer is on
		/// </summary>
		private MapFile ActiveMap
		{
			get
			{
				if (MapCache.Count == 0 || !MapCache.ContainsKey(MainPlayer.ActiveCharacter.CurrentMap))
				{
					return _tryLoadMap(MainPlayer.ActiveCharacter.CurrentMap) ? MapCache[MainPlayer.ActiveCharacter.CurrentMap] : null;
				}
				return MapCache[MainPlayer.ActiveCharacter.CurrentMap];
			}
		}

		private EOMapRenderer m_mapRender;
		/// <summary>
		/// Returns a map rendering object encapsulating the map the MainPlayer is on
		/// </summary>
		public EOMapRenderer ActiveMapRenderer
		{
			get
			{
				//lazy initialization
				if (m_mapRender == null)
				{
					m_mapRender = new EOMapRenderer(EOGame.Instance, ActiveMap, m_api);
				}

				//make sure it's in the game's componenets
				if(EOGame.Instance.State == GameStates.PlayingTheGame && !EOGame.Instance.Components.Contains(m_mapRender))
					EOGame.Instance.Components.Add(m_mapRender);

				return m_mapRender;
			}
		}

		private EOCharacterRenderer m_charRender;

		/// <summary>
		/// Returns a reference to the primary EOCharacterRenderer associated with MainPlayer.ActiveCharacter
		/// </summary>
		public EOCharacterRenderer ActiveCharacterRenderer
		{
			get
			{
				//lazy initialization
				if (m_charRender == null)
				{
					m_charRender = new EOCharacterRenderer(MainPlayer.ActiveCharacter);
					m_charRender.Initialize();
				}

				EOCharacterRenderer ret = m_charRender;

				if(ret.Character == null)
				{
					EOGame.Instance.LostConnectionDialog();
					return null;
				}

				//if player logs out and logs back in
				if (ret.Character != MainPlayer.ActiveCharacter)
				{
					ret.Dispose();
					ret = m_charRender = new EOCharacterRenderer(MainPlayer.ActiveCharacter);
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
		
		private readonly AsyncClient m_client;
		public AsyncClient Client
		{
			get { return m_client; }
		}

		private PacketAPI m_api;
		public void SetAPIHandle(PacketAPI api) { m_api = api; }

		/*** Functions for loading/checking the different pub/map files ***/

		//tries to load the map that MainPlayer.ActiveCharacter is hanging out on
		private bool _tryLoadMap(int mapID = -1)
		{
			try
			{
				if (mapID < 0)
					mapID = MainPlayer.ActiveCharacter.CurrentMap;

				string mapFile = Path.Combine("maps", string.Format("{0,5:D5}.emf", mapID));

				if(!MapCache.ContainsKey(mapID))
					MapCache.Add(mapID, new MapFile(mapFile));
				else
					MapCache[mapID] = new MapFile(mapFile);
			}
			catch
			{
				return false;
			}

			return true;
		}

		private bool _tryLoadItems(string fileName = null)
		{
			try
			{
				m_items = string.IsNullOrEmpty(fileName) ? new ItemFile() : new ItemFile(fileName);
			}
			catch
			{
				m_items = null;
				return false;
			}

			return true;
		}

		private bool _tryLoadNPCs(string fileName = null)
		{
			try
			{
				m_npcs = string.IsNullOrEmpty(fileName) ? new NPCFile() : new NPCFile(fileName);
			}
			catch
			{
				m_npcs = null;
				return false;
			}

			return true;
		}

		private bool _tryLoadSpells(string fileName = null)
		{
			try
			{
				m_spells = string.IsNullOrEmpty(fileName) ? new SpellFile() : new SpellFile(fileName);
			}
			catch
			{
				m_spells = null;
				return false;
			}

			return true;
		}

		private bool _tryLoadClasses(string fileName = null)
		{
			try
			{
				m_classes = string.IsNullOrEmpty(fileName) ? new ClassFile() : new ClassFile(fileName);
			}
			catch
			{
				m_classes = null;
				return false;
			}

			return true;
		}

		public bool CheckMap(short mapID, byte[] mapRid, int mapFileSize)
		{
			NeedMap = -1;

			string mapFile = string.Format("maps\\{0,5:D5}.emf", mapID);
			if (!Directory.Exists("maps") || !File.Exists(mapFile))
			{
				Directory.CreateDirectory("maps");
				NeedMap = mapID;
				return false;
			}

			//try to load the map if it isn't cached. on failure, set needmap
			if (!MapCache.ContainsKey(mapID))
				NeedMap = _tryLoadMap(mapID) ? -1 : mapID;

			//on success of file load, check the rid and the size of the file
			if (MapCache.ContainsKey(mapID))
			{
				for (int i = 0; i < 4; ++i)
				{
					if(MapCache[mapID].Rid[i] != mapRid[i])
					{
						NeedMap = mapID;
						break;
					}
				}

				if (NeedMap == -1 && MapCache[mapID].FileSize != mapFileSize)
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

		public void WarpAgreeAction(short mapID, WarpAnimation anim, List<CharacterData> chars, List<NPCData> npcs, List<MapItem> items)
		{
			if (mapID >= 0)
			{
				if (!_tryLoadMap(mapID))
				{
					EOGame.Instance.LostConnectionDialog();
					EODialog.Show("Something went wrong when loading the map. Try logging in again.", "Map Load Error");
				}

				MainPlayer.ActiveCharacter.CurrentMap = mapID;
				ActiveMapRenderer.SetActiveMap(ActiveMap);
			}

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

			foreach (MapItem mi in items)
				ActiveMapRenderer.AddMapItem(mi);
		}

		public void ApplyWelcomeRequest(PacketAPI api, WelcomeRequestData data)
		{
			MainPlayer.SetPlayerID(data.PlayerID);
			MainPlayer.SetActiveCharacter(api, data.ActiveCharacterID);
			MainPlayer.ActiveCharacter.CurrentMap = data.MapID;
			MainPlayer.ActiveCharacter.MapIsPk = data.MapIsPK;

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
				level = data.Level,
				exp = data.Exp,
				usage = data.Usage,

				hp = data.HP,
				maxhp = data.MaxHP,
				tp = data.TP,
				maxtp = data.MaxTP,
				sp = data.MaxSP,
				maxsp = data.MaxSP,

				statpoints = data.StatPoints,
				skillpoints = data.SkillPoints,
				mindam = data.MinDam,
				maxdam = data.MaxDam,
				karma = data.Karma,
				accuracy = data.Accuracy,
				evade = data.Evade,
				armor = data.Armor,
				disp_str = data.DispStr,
				disp_int = data.DispInt,
				disp_wis = data.DispWis,
				disp_agi = data.DispAgi,
				disp_con = data.DispCon,
				disp_cha = data.DispCha
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

			ActiveMapRenderer.ClearOtherPlayers();
			ActiveMapRenderer.ClearOtherNPCs();
			ActiveMapRenderer.ClearMapItems();

			foreach (var character in data.CharacterData)
			{
				if (character.Name.ToLower() == MainPlayer.ActiveCharacter.Name.ToLower())
					MainPlayer.ActiveCharacter.ApplyData(character, false); //do NOT copy paperdoll data over the existing!
				else
					ActiveMapRenderer.AddOtherPlayer(character);
			}

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

		private Dictionary<FamilyActionPair, LockedHandlerMethod> _getPacketHandlerDictionary()
		{
			return new Dictionary<FamilyActionPair, LockedHandlerMethod>
			{
				{
					new FamilyActionPair(PacketFamily.Effect, PacketAction.Player),
					new LockedHandlerMethod(Effect.EffectPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Emote, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Emote.EmotePlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Buy),
					new LockedHandlerMethod(Locker.LockerBuy, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Get),
					new LockedHandlerMethod(Locker.LockerGet, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Open),
					new LockedHandlerMethod(Locker.LockerOpen, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Reply),
					new LockedHandlerMethod(Locker.LockerReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Buy),
					new LockedHandlerMethod(Shop.ShopBuy, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Create),
					new LockedHandlerMethod(Shop.ShopCreate, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Open),
					new LockedHandlerMethod(Shop.ShopOpen, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Sell),
					new LockedHandlerMethod(Shop.ShopSell, true)
				},
				{
					new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Player),
					new LockedHandlerMethod(StatSkill.StatSkillPlayer, true)
				}
			};
		}

		public void Remap()
		{
			MapCache.Remove(MainPlayer.ActiveCharacter.CurrentMap);
			if (!_tryLoadMap())
				throw new FileLoadException("Unable to load remapped map file!");
			ActiveMapRenderer.SetActiveMap(MapCache[MainPlayer.ActiveCharacter.CurrentMap]);
		}
	}
}
