//****************************************************************************************************
//* This is a modified version of an original work licensed under the GPL v3.0
//* Original unmodifeid source is available at https://www.assembla.com/code/eo-dev-sharp/subversion/nodes (see Data/EMF.cs)
//* For additional details on GPL v3.0 see the file GPL3License.txt
//****************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EOLib.Data.Map;
using EOLib.Net;

namespace EOLib.IO
{
	public enum EODirection : byte
	{
		Down = 0,
		Left,
		Up,
		Right,
		Invalid = 255
	}

	public enum TileSpec : byte
	{
		Wall = 0,
		ChairDown = 1,
		ChairLeft = 2,
		ChairRight = 3,
		ChairUp = 4,
		ChairDownRight = 5,
		ChairUpLeft = 6,
		ChairAll = 7,
		JammedDoor = 8,
		Chest = 9,
		BankVault = 16,
		NPCBoundary = 17,
		MapEdge = 18,
		FakeWall = 19,
		Board1 = 20,
		Board2 = 21,
		Board3 = 22,
		Board4 = 23,
		Board5 = 24,
		Board6 = 25,
		Board7 = 26,
		Board8 = 27,
		Jukebox = 28,
		Jump = 29,
		Water = 30,
		Arena = 32,
		AmbientSource = 33,
		SpikesTimed = 34,
		SpikesStatic = 35,
		SpikesTrap = 36,

		None = 255
	}

	public enum MapEffect : byte
	{
		None = 0,
		HPDrain = 1,
		TPDrain = 2,
		Quake = 3
	}

	public enum DoorSpec : short
	{
		NoDoor,
		Door,
		LockedSilver,
		LockedCrystal,
		LockedWraith
	}

	public enum ChestKey : short
	{
		None,
		Normal,
		Silver,
		Crystal,
		Wraith
	}

	public class NPCSpawn
	{
		public byte x;
		public byte y;
		public short id;
		public byte index;
		public byte spawnType;
		public short spawnTime;
		public byte amount;

		public EODirection direction;
	}
	
	public class MapChest
	{
		public byte x;
		public byte y;
		public ChestKey key;
		public byte slot;
		public short item;
		public short time;
		public int amount;
	}

	public struct MapItem
	{
		public short uid;
		public short id;
		public byte x;
		public byte y;
		public int amount;
		public DateTime time;
		public bool npcDrop;
		public int playerID;
	}

	public class Tile
	{
		public byte x;
		public TileSpec spec;
	}

	public struct TileRow
	{
		public byte y;
		public List<Tile> tiles;
	}

	public class Warp : IMapElement
	{
		public byte x;
		public short warpMap;
		public byte warpX;
		public byte warpY;
		public byte levelRequirement;
		public DoorSpec door;
		public bool doorOpened;
		public bool backOff; //used in code only: determines whether a door packet was recently sent for this particular door (only valid for doors)
	}

	public struct WarpRow
	{
		public byte y;
		public List<Warp> tiles;
	}

	public class GFX
	{
		public byte x;
		public ushort tile;
	}

	public struct GFXRow
	{
		public byte y;
		public List<GFX> tiles;
	}

	public struct MapSign : IMapElement
	{
		public byte x;
		public byte y;
		public string title;
		public string message;
	}

	public enum MapLayers
	{
		GroundTile = 0,
		Objects = 1,
		OverlayObjects = 2,
		WallRowsDown = 3,
		WallRowsRight = 4,
		Unknown = 5,
		OverlayTile = 6,
		Shadow = 7,
		Roof = 8,
		NUM_LAYERS = 9
	}

	public class MapFile
	{
		//lookup tables for direct access so I don't need so many 'find's

		public Warp[,] WarpLookup { get; private set; }
		public Tile[,] TileLookup { get; private set; }
		public List<int[,]> GFXLookup {get; private set; }

		public int MapID { get; private set; }

		public int FileSize { get; private set; }
		public byte[] Rid { get; private set; }
		public string Name { get; private set; }

		public bool IsPK { get; private set; }
		private byte pkByte;
		public MapEffect Effect { get; private set; }

		public byte Music { get; private set; }
		public byte MusicExtra { get; private set; }
		public short AmbientNoise { get; private set; }

		public byte Width { get; private set; }
		public byte Height { get; private set; }

		public short FillTile { get; private set; }
		public bool MapAvailable { get; private set; }
		private byte availByte;
		public bool CanScroll { get; private set; }
		private byte scrollByte;
		public byte RelogX { get; private set; }
		public byte RelogY { get; private set; }
		private byte Unknown2 { get; set; }

		public List<NPCSpawn> NPCSpawns { get; private set; }
		private List<byte[]> Unknowns { get; set; }

		public List<MapChest> Chests { get; private set; }
		public List<TileRow> TileRows { get; private set; }
		public List<WarpRow> WarpRows { get; private set; }
		public List<GFXRow>[] GfxRows { get; private set; }
		public List<MapSign> Signs { get; private set; }

		public bool HasTimedSpikes { get; private set; }
		
		private static string _decodeMapString(byte[] chars)
		{
			Array.Reverse(chars);

			bool flippy = (chars.Length % 2) == 1;

			for (int i = 0; i < chars.Length; ++i)
			{
				byte c = chars[i];
				if (c == 0xFF)
				{
					Array.Resize(ref chars, i);
					break;
				}

				if (flippy)
				{
					if (c >= 0x22 && c <= 0x4F)
						c = (byte)(0x71 - c);
					else if (c >= 0x50 && c <= 0x7E)
						c = (byte)(0xCD - c);
				}
				else
				{
					if (c >= 0x22 && c <= 0x7E)
						c = (byte)(0x9F - c);
				}

				chars[i] = c;
				flippy = !flippy;
			}

			return Encoding.ASCII.GetString(chars);
		}

		private static byte[] _encodeMapString(string s)
		{
			byte[] chars = Encoding.ASCII.GetBytes(s);
			bool flippy = (chars.Length % 2) == 1;
			int i;
			for (i = 0; i < chars.Length; ++i)
			{
				byte c = chars[i];
				if (flippy)
				{
					if (c >= 0x22 && c <= 0x4F)
						c = (byte) (0x71 - c);
					else if (c >= 0x50 && c <= 0x7E)
						c = (byte) (0xCD - c);
				}
				else
				{
					if (c >= 0x22 && c <= 0x7E)
						c = (byte) (0x9F - c);
				}
				chars[i] = c;
				flippy = !flippy;
			}
			Array.Reverse(chars);
			return chars;
		}

		public MapFile(string fileName)
		{
			string id = fileName.Substring(fileName.LastIndexOf('\\') + 1, 5);
			MapID = int.Parse(id);
			Load(fileName);
		}

		private void Load(string fileName)
		{
			Rid = new byte[4];
			Name = String.Empty;
			NPCSpawns = new List<NPCSpawn>();
			Unknowns = new List<byte[]>();
			Chests = new List<MapChest>();
			TileRows = new List<TileRow>();
			WarpRows = new List<WarpRow>();
			GfxRows = new List<GFXRow>[(int)MapLayers.NUM_LAYERS];
			Signs = new List<MapSign>();

			for (int layer = (int)MapLayers.GroundTile; layer < (int)MapLayers.NUM_LAYERS; ++layer)
			{
				GfxRows[layer] = new List<GFXRow>();
			}

			OldPacket file = new OldPacket(File.ReadAllBytes(fileName));
			if (file.Length == 0)
				throw new FileLoadException("The file is empty.");
			file.ReadPos = 0; //use packet wrapper for convenience functions/decoding

			if (file.GetFixedString(3) != "EMF")
				throw new Exception("Corrupt or not an EMF file");

			FileSize = file.Length;

			Rid = file.GetBytes(4);
			byte[] rawname = file.GetBytes(24);
			Name = _decodeMapString(rawname);

			//account for override in Rid to make a map PK - byte 0 is 0xff and byte 1 is 0x01
			IsPK = (pkByte = file.GetChar()) == 3 || (Rid[0] == 0xff && Rid[1] == 0x01);
			Effect = (MapEffect)file.GetChar();
			Music = file.GetChar();
			MusicExtra = file.GetChar();
			AmbientNoise = file.GetShort();
			Width = file.GetChar();
			Height = file.GetChar();
			FillTile = file.GetShort();
			MapAvailable = (availByte = file.GetChar()) == 1;
			CanScroll = (scrollByte = file.GetChar()) == 1;
			RelogX = file.GetChar();
			RelogY = file.GetChar();
			Unknown2 = file.GetChar();

			WarpLookup = new Warp[Height + 1, Width + 1];
			TileLookup = new Tile[Height + 1, Width + 1];

			int innersize;
			int outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				NPCSpawns.Add(new NPCSpawn
				{
					x = file.GetChar(),
					y = file.GetChar(),
					id = file.GetShort(),
					spawnType = file.GetChar(),
					spawnTime = file.GetShort(),
					amount = file.GetChar()
				});
			}

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				Unknowns.Add(file.GetBytes(4));
			}

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				Chests.Add(new MapChest
				{
					x = file.GetChar(),
					y = file.GetChar(),
					key = (ChestKey)file.GetShort(),
					slot = file.GetChar(),
					item = file.GetShort(),
					time = file.GetShort(),
					amount = file.GetThree()
				});
			}

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				byte y = file.GetChar();
				innersize = file.GetChar();

				TileRow row = new TileRow
				{
					y = y,
					tiles = new List<Tile>(innersize)
				};

				for (int ii = 0; ii < innersize; ++ii)
				{
					Tile t;
					row.tiles.Add(t = new Tile
					{
						x = file.GetChar(),
						spec = (TileSpec)file.GetChar()
					});
					if (t.spec == TileSpec.SpikesTimed)
						HasTimedSpikes = true;
					TileLookup[row.y, t.x] = t;
				}

				TileRows.Add(row);
			}

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				byte y = file.GetChar();
				innersize = file.GetChar();

				WarpRow row = new WarpRow { y = y, tiles = new List<Warp>(innersize) };

				for (int ii = 0; ii < innersize; ++ii)
				{
					Warp w;
					row.tiles.Add(w = new Warp
					{
						x = file.GetChar(),
						warpMap = file.GetShort(),
						warpX = file.GetChar(),
						warpY = file.GetChar(),
						levelRequirement = file.GetChar(),
						door = (DoorSpec)file.GetShort()
					});

					if (row.y <= Height && w.x <= Width)
						WarpLookup[row.y, w.x] = w;
				}

				WarpRows.Add(row);
			}

			GFXLookup = new List<int[,]>((int)MapLayers.NUM_LAYERS);
			for (int i = 0; i < (int)MapLayers.NUM_LAYERS; ++i)
			{
				GFXLookup.Add(new int[Height + 1, Width + 1]);
				for (int row = 0; row <= Height; row++)
				{
					for (int col = 0; col <= Width; col++)
					{
						GFXLookup[i][row, col] = -1;
					}
				}
			}

			for (int layer = 0; layer < (int)MapLayers.NUM_LAYERS; ++layer)
			{
				if (file.ReadPos == file.Length)
					break;

				outersize = file.GetChar();
				GfxRows[layer] = new List<GFXRow>(outersize);

				for (int i = 0; i < outersize; ++i)
				{
					byte y = file.GetChar();
					innersize = file.GetChar();

					GFXRow row = new GFXRow { y = y, tiles = new List<GFX>(innersize) };

					for (int ii = 0; ii < innersize; ++ii)
					{
						byte tempx = file.GetChar();
						ushort temptile = (ushort)file.GetShort();
						if (row.y <= Height && tempx <= Width)
						{
							GFXLookup[layer][row.y, tempx] = temptile;
						}
						row.tiles.Add(new GFX { x = tempx, tile = temptile });
					}

					GfxRows[layer].Add(row);
				}
			}

			if (file.ReadPos == file.Length)
				return;

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				MapSign sign = new MapSign {x = file.GetChar(), y = file.GetChar()};

				int msgLength = file.GetShort() - 1;
				string data = _decodeMapString(file.GetBytes(msgLength));
				int titleLength = file.GetChar();
				sign.title = data.Substring(0, titleLength);
				sign.message = data.Substring(titleLength);

				Signs.Add(sign);
			}
		}

		public void Save(string fileName)
		{
			OldPacket file = new OldPacket(PacketFamily.Internal, PacketAction.Server) {ReadPos = 0, WritePos = 0};

			//map header
			file.AddString("EMF");
			file.AddBytes(Rid);

			byte[] tmpName = _encodeMapString(Name);
			byte[] rawName = new byte[24];
			for (int i = rawName.Length - 1; i >= 0; --i) rawName[i] = 0xFF;
			Array.Copy(tmpName, 0, rawName, rawName.Length - tmpName.Length, tmpName.Length);
			file.AddBytes(rawName);

			file.AddChar(pkByte);
			file.AddChar((byte)Effect);
			file.AddChar(Music);
			file.AddChar(MusicExtra);
			file.AddShort(AmbientNoise);
			file.AddChar(Width);
			file.AddChar(Height);
			file.AddShort(FillTile);
			file.AddChar(availByte);
			file.AddChar(scrollByte);
			file.AddChar(RelogX);
			file.AddChar(RelogY);
			file.AddChar(Unknown2);

			//NPC Spawns
			file.AddChar((byte)NPCSpawns.Count);
			foreach (NPCSpawn spawn in NPCSpawns)
			{
				file.AddChar(spawn.x);
				file.AddChar(spawn.y);
				file.AddShort(spawn.id);
				file.AddChar(spawn.spawnType);
				file.AddShort(spawn.spawnTime);
				file.AddChar(spawn.amount);
			}

			//unknowns
			file.AddChar((byte)Unknowns.Count);
			foreach(byte[] b in Unknowns)
				file.AddBytes(b);

			//chests
			file.AddChar((byte)Chests.Count);
			foreach (MapChest chest in Chests)
			{
				file.AddChar(chest.x);
				file.AddChar(chest.y);
				file.AddShort((short)chest.key);
				file.AddChar(chest.slot);
				file.AddShort(chest.item);
				file.AddShort(chest.time);
				file.AddThree(chest.amount);
			}

			//tile specs
			file.AddChar((byte)TileRows.Count);
			foreach (TileRow tr in TileRows)
			{
				file.AddChar(tr.y);
				file.AddChar((byte)tr.tiles.Count);
				foreach (Tile tt in tr.tiles)
				{
					file.AddChar(tt.x);
					file.AddChar((byte)tt.spec);
				}
			}

			//warps
			file.AddChar((byte)WarpRows.Count);
			foreach (WarpRow wr in WarpRows)
			{
				file.AddChar(wr.y);
				file.AddChar((byte)wr.tiles.Count);
				foreach (Warp ww in wr.tiles)
				{
					file.AddChar(ww.x);
					file.AddShort(ww.warpMap);
					file.AddChar(ww.warpX);
					file.AddChar(ww.warpY);
					file.AddChar(ww.levelRequirement);
					file.AddShort((short)ww.door);
				}
			}

			//gfx
			for (int layer = 0; layer < (int) MapLayers.NUM_LAYERS; ++layer)
			{
				file.AddChar((byte)GfxRows[layer].Count);
				foreach (GFXRow row in GfxRows[layer])
				{
					file.AddChar(row.y);
					file.AddChar((byte)row.tiles.Count);
					foreach (GFX gfx in row.tiles)
					{
						file.AddChar(gfx.x);
						file.AddShort((short)gfx.tile);
					}
				}
			}

			//signs
			file.AddChar((byte)Signs.Count);
			foreach (MapSign sign in Signs)
			{
				file.AddChar(sign.x);
				file.AddChar(sign.y);
				file.AddShort((short)(sign.message.Length + sign.title.Length + 1));
				
				byte[] fileMsg = new byte[sign.message.Length + sign.title.Length];
				byte[] rawTitle = _encodeMapString(sign.title);
				byte[] rawMessage = _encodeMapString(sign.message);
				Array.Copy(rawTitle, fileMsg, fileMsg.Length);
				Array.Copy(rawMessage, 0, fileMsg, rawTitle.Length, rawMessage.Length);
				file.AddBytes(fileMsg);
				file.AddChar((byte)rawTitle.Length);
			}

			File.WriteAllBytes(fileName, file.Data);
		}
	}
}