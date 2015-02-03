using System;
using System.Collections.Generic;
using System.IO;
using EOLib;
using EOLib.Data;

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
		private static Microsoft.Xna.Framework.Graphics.SpriteFont dbg;
		public static Microsoft.Xna.Framework.Graphics.SpriteFont DBG
		{
			get
			{
				if (EOGame.Instance == null) return null;
				return dbg ?? (dbg = EOGame.Instance.Content.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>("dbg"));
			}
		}
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

		private World() //don't allow construction of the world using 'new'
		{
			TryLoadItems();
			TryLoadNPCs();
			TryLoadSpells();
			TryLoadClasses();
			
			//initial capacity of 32: most players won't travel between too many maps in a gaming session
			MapCache = new Dictionary<int, EOLib.MapFile>(32);
			m_player = new Player();
			m_client = new EOClient();
			m_config = new IniReader(@"config\settings.ini");
			if (!m_config.Load())
				throw new WorldLoadException("Unable to load the configuration file!");
		}

		public void Init()
		{
			Initialized = true;

			exp_table = new int[254];
			for (int i = 1; i < exp_table.Length; ++i)
			{
				exp_table[i] = (int)Math.Round(Math.Pow(i, 3) * 133.1);
			}

			int maj, min, cli;

			VersionMajor = Configuration.GetValue(ConfigStrings.Version, ConfigStrings.Major, out maj) ? (byte)maj : Constants.MajorVersion;
			VersionMinor = Configuration.GetValue(ConfigStrings.Version, ConfigStrings.Minor, out min) ? (byte)min : Constants.MinorVersion;
			VersionClient = Configuration.GetValue(ConfigStrings.Version, ConfigStrings.Client, out cli) ? (byte)cli : Constants.ClientVersion;
		}

		public int[] exp_table;

		/*** Instance Properties and such ***/

		public byte VersionMajor { get; set; }
		public byte VersionMinor { get; set; }
		public byte VersionClient { get; set; }

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
		private Dictionary<int, EOLib.MapFile> MapCache { get; set; }

		/// <summary>
		/// Returns a MapFile for the map the MainPlayer is on
		/// </summary>
		private EOLib.MapFile ActiveMap
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

		private readonly IniReader m_config;
		public IniReader Configuration
		{
			get { return m_config; }
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

				MapCache.Add(mapID, new MapFile(mapFile));
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

		public void CheckMap(int mapID, byte[] mapRid, int mapFileSize)
		{
			NeedMap = -1;

			string mapFile = string.Format("maps\\{0,5:D5}.emf", mapID);
			if (!Directory.Exists("maps") || !File.Exists(mapFile))
			{
				Directory.CreateDirectory("maps");
				NeedMap = mapID;
				return;
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
		}

		public void CheckPub(Handlers.InitFileType file, int rid, short len)
		{
			const string fName = "pub\\";
			if (!Directory.Exists(fName))
				Directory.CreateDirectory(fName);

			switch (file)
			{
				case Handlers.InitFileType.Item:
					NeedEIF = !TryLoadItems();
					if(EIF != null)
						NeedEIF = rid != EIF.Rid || len != EIF.Len;
					break;
				case Handlers.InitFileType.Npc:
					NeedENF = !TryLoadNPCs();
					if (ENF != null)
						NeedENF = rid != ENF.Rid || len != ENF.Len;
					break;
				case Handlers.InitFileType.Spell:
					NeedESF = !TryLoadSpells();
					if (ESF != null)
						NeedESF = rid != ESF.Rid || len != ESF.Len;
					break;
				case Handlers.InitFileType.Class:
					NeedECF = !TryLoadClasses();
					if (ECF != null)
						NeedECF = rid != ECF.Rid || len != ECF.Len;
					break;
				default:
					return;
			}
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
			if (!disposing) return;

			if(m_mapRender != null)
				m_mapRender.Dispose();

			if(m_charRender != null)
				m_charRender.Dispose();

			if(m_client != null)
				m_client.Dispose();
		}
	}
}
