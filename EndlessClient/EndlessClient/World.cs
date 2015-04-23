using System;
using System.Collections.Generic;
using System.IO;
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
		public static string GetString(DATCONST1 id)
		{
			return Instance.DataFiles[Instance.Localized1].Data[(int) id];
		}
		public static string GetString(DATCONST2 id)
		{
			return Instance.DataFiles[Instance.Localized2].Data[(int)id];
		}

		private World() //private: don't allow construction of the world using 'new'
		{
			TryLoadItems();
			TryLoadNPCs();
			TryLoadSpells();
			TryLoadClasses();
			
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
					return TryLoadMap(MainPlayer.ActiveCharacter.CurrentMap) ? MapCache[MainPlayer.ActiveCharacter.CurrentMap] : null;
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
					m_mapRender = new EOMapRenderer(EOGame.Instance, ActiveMap);
				}

				//check for an update on the map file any time the Map renderer is accessed
				if (m_mapRender.MapRef == null || m_mapRender.MapRef.MapID != MainPlayer.ActiveCharacter.CurrentMap)
				{
					m_mapRender.SetActiveMap(ActiveMap);
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

		/*** Functions for loading/checking the different pub/map files ***/

		//tries to load the map that MainPlayer.ActiveCharacter is hanging out on
		public bool TryLoadMap(int mapID = -1)
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

		public bool TryLoadItems(string fileName = null)
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

		public bool TryLoadNPCs(string fileName = null)
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

		public bool TryLoadSpells(string fileName = null)
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

		public bool TryLoadClasses(string fileName = null)
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
				NeedMap = TryLoadMap(mapID) ? -1 : mapID;

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

		public void CheckPub(InitFileType file, int rid, short len)
		{
			const string fName = "pub\\";
			if (!Directory.Exists(fName))
				Directory.CreateDirectory(fName);

			switch (file)
			{
				case InitFileType.Item:
					NeedEIF = !TryLoadItems();
					if(EIF != null)
						NeedEIF = rid != EIF.Rid || len != EIF.Len;
					break;
				case InitFileType.Npc:
					NeedENF = !TryLoadNPCs();
					if (ENF != null)
						NeedENF = rid != ENF.Rid || len != ENF.Len;
					break;
				case InitFileType.Spell:
					NeedESF = !TryLoadSpells();
					if (ESF != null)
						NeedESF = rid != ESF.Rid || len != ESF.Len;
					break;
				case InitFileType.Class:
					NeedECF = !TryLoadClasses();
					if (ECF != null)
						NeedECF = rid != ECF.Rid || len != ECF.Len;
					break;
				default:
					return;
			}
		}

		public void WarpAgreeAction(short mapID, WarpAnimation anim, List<CharacterData> chars, List<NPCData> npcs, List<MapItem> items)
		{
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

		public void ApplyWelcomeRequest(WelcomeRequestData data)
		{
			MainPlayer.SetPlayerID(data.PlayerID);
			MainPlayer.SetActiveCharacter(data.ActiveCharacterID);
			MainPlayer.ActiveCharacter.CurrentMap = data.MapID;
			MainPlayer.ActiveCharacter.MapIsPk = data.MapIsPK;

			CheckMap(data.MapID, data.MapRID, data.MapLen);
			CheckPub(InitFileType.Item, data.EifRid, data.EifLen);
			CheckPub(InitFileType.Npc, data.EnfRid, data.EnfLen);
			CheckPub(InitFileType.Spell, data.EsfRid, data.EsfLen);
			CheckPub(InitFileType.Class, data.EcfRid, data.EcfLen);

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

				Handlers.Character.Cleanup();
				Handlers.Login.Cleanup();
				Handlers.Walk.Cleanup();
			}
		}

		private Dictionary<FamilyActionPair, LockedHandlerMethod> _getPacketHandlerDictionary()
		{
			return new Dictionary<FamilyActionPair, LockedHandlerMethod>
			{
				{
					new FamilyActionPair(PacketFamily.AdminInteract, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.AdminInteract.AdminShow)
				},
				{
					new FamilyActionPair(PacketFamily.AdminInteract, PacketAction.Remove),
					new LockedHandlerMethod(Handlers.AdminInteract.AdminHide)
				},
				{
					new FamilyActionPair(PacketFamily.Attack, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Attack.AttackPlayerResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Avatar, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Avatar.AvatarAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.Avatar, PacketAction.Remove),
					new LockedHandlerMethod(Handlers.Avatar.AvatarRemove, true)
				},
				{
					new FamilyActionPair(PacketFamily.Bank, PacketAction.Open), 
					new LockedHandlerMethod(Handlers.Bank.BankOpenReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Bank, PacketAction.Reply), 
					new LockedHandlerMethod(Handlers.Bank.BankReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Character, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Character.CharacterPlayerResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Character, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Character.CharacterResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Chest, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Chest.ChestAgreeResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Chest, PacketAction.Get),
					new LockedHandlerMethod(Handlers.Chest.ChestGetResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Chest, PacketAction.Open),
					new LockedHandlerMethod(Handlers.Chest.ChestOpenResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Chest, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Chest.ChestReply)
				},
				{
					new FamilyActionPair(PacketFamily.Connection, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Connection.PingResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Door, PacketAction.Open),
					new LockedHandlerMethod(Handlers.Door.DoorOpenResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Effect, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Effect.EffectPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Emote, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Emote.EmotePlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Face, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Face.FacePlayerResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Add),
					new LockedHandlerMethod(Handlers.Item.ItemAddResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Drop),
					new LockedHandlerMethod(Handlers.Item.ItemDropResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Get),
					new LockedHandlerMethod(Handlers.Item.ItemGetResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Junk),
					new LockedHandlerMethod(Handlers.Item.ItemJunkResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Remove),
					new LockedHandlerMethod(Handlers.Item.ItemRemoveResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Item.ItemReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Buy),
					new LockedHandlerMethod(Handlers.Locker.LockerBuy, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Get),
					new LockedHandlerMethod(Handlers.Locker.LockerGet, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Open),
					new LockedHandlerMethod(Handlers.Locker.LockerOpen, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Locker.LockerReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Login, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Login.LoginResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Message, PacketAction.Pong), 
					new LockedHandlerMethod(Handlers.Message.Pong)
				},
				{
					new FamilyActionPair(PacketFamily.NPC, PacketAction.Accept),
					new LockedHandlerMethod(Handlers.NPCPackets.NPCAccept, true)
				},
				{
					new FamilyActionPair(PacketFamily.NPC, PacketAction.Player),
					new LockedHandlerMethod(Handlers.NPCPackets.NPCPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.NPC, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.NPCPackets.NPCReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.NPC, PacketAction.Spec),
					new LockedHandlerMethod(Handlers.NPCPackets.NPCSpec, true)
				},
				{
					new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Paperdoll.PaperdollAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Remove),
					new LockedHandlerMethod(Handlers.Paperdoll.PaperdollRemove, true)
				},
				{
					new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Paperdoll.PaperdollReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Ping),
					new LockedHandlerMethod(Handlers.Players.PlayersPing, true)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Pong),
					new LockedHandlerMethod(Handlers.Players.PlayersPong, true)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Net3),
					new LockedHandlerMethod(Handlers.Players.PlayersNet3, true)
				},
				{
					new FamilyActionPair(PacketFamily.Recover, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Recover.RecoverAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.Recover, PacketAction.List),
					new LockedHandlerMethod(Handlers.Recover.RecoverList, true)
				},
				{
					new FamilyActionPair(PacketFamily.Recover, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Recover.RecoverPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Recover, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Recover.RecoverReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Buy),
					new LockedHandlerMethod(Handlers.Shop.ShopBuy, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Create),
					new LockedHandlerMethod(Handlers.Shop.ShopCreate, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Open),
					new LockedHandlerMethod(Handlers.Shop.ShopOpen, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Sell),
					new LockedHandlerMethod(Handlers.Shop.ShopSell, true)
				},
				{
					new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Player),
					new LockedHandlerMethod(Handlers.StatSkill.StatSkillPlayer, true)
				},
				//TALK PACKETS
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Message),
					new LockedHandlerMethod(Handlers.Talk.TalkMessage, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Talk.TalkPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Talk.TalkReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Request),
					new LockedHandlerMethod(Handlers.Talk.TalkRequest, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Server),
					new LockedHandlerMethod(Handlers.Talk.TalkServer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Tell),
					new LockedHandlerMethod(Handlers.Talk.TalkTell, true)
				},
				//
				{
					new FamilyActionPair(PacketFamily.Walk, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Walk.WalkReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Walk, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Walk.WalkPlayer, true)
				}
			};
		}
	}
}
