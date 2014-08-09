//****************************************************************************************************
//* Credit for this file goes to DeathX/Exile Studios
//* Source taken from https://www.assembla.com/code/eo-dev-sharp/subversion/nodes (see Data/EMF.cs)
//* Refactored a little bit to meet some coding standards of mine
//****************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace EOLib
{
	public enum EODirection : byte
	{
		Down = 0,
		Left,
		Up,
		Right
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
		Spikes = 34,
		SpikesTrap = 35,
		SpikesTimed = 36,

		None = 255
	}

	public enum MapEffect : byte
	{
		None = 0,
		HPDrain = 1,
		TPDrain = 2,
		Quake = 3
	}

	public class MapNPC
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
	
	public struct MapChest
	{
		public byte x;
		public byte y;
		public short key;
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
	}

	public struct Tile
	{
		public byte x;
		public TileSpec spec;
	}

	public struct TileRow
	{
		public byte y;
		public List<Tile> tiles;
	}

	public class Warp
	{
		public byte x;
		public short warpMap;
		public byte warpX;
		public byte warpY;
		public byte levelRequirement;
		public short door;
		public bool doorOpened;

		public static readonly Warp Empty = new Warp();
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

	public struct ESGFXColumn
	{
		public int tile;
	}

	public struct ESGFXRow
	{
		public ESGFXColumn[] column;
	}

	public struct MapSign
	{
		public byte x;
		public byte y;
		public string title;
		public string message;
	}

	public class MapFile
	{
		public ESGFXRow[,] GFXRows;
		
		private const int GFXLayers = 9;

		public int MapID { get; private set; }

		public int FileSize { get; private set; }
		public byte[] Rid { get; private set; }
		public string Name { get; private set; }

		public bool IsPK { get; private set; }
		public MapEffect Effect { get; private set; }

		public byte Music { get; private set; }
		public byte MusicExtra { get; private set; }
		public short AmbientNoise { get; private set; }

		public byte Width { get; private set; }
		public byte Height { get; private set; }

		public short FillTile { get; private set; }
		public byte MapAvailable { get; private set; }
		public byte CanScroll { get; private set; }
		public byte RelogX { get; private set; }
		public byte RelogY { get; private set; }
		public byte Unknown2 { get; private set; }

		public List<MapNPC> MapNpcs { get; set; }
		public List<byte[]> Unknowns { get; set; }

		public List<MapChest> Chests { get; set; }
		public List<TileRow> TileRows { get; set; }
		public List<WarpRow> WarpRows { get; set; }
		public List<GFXRow>[] GfxRows { get; set; }
		public List<MapSign> Signs { get; set; }
		
		private string decodeEmfString(byte[] chars)
		{
			Array.Reverse(chars);

			bool flippy = (chars.Length % 2) == 1;

			for (int i = 0; i < chars.Length; ++i)
			{
				byte c = chars[i];

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

			return ASCIIEncoding.ASCII.GetString(chars);
		}

		public MapFile(int id)
		{
			MapID = id;
			Read(Path.Combine("maps\\{0,5:D5}.emf"));
		}

		public MapFile(string fileName)
		{
			MapID = 0;
			Read(fileName);
		}

		public void Read(string fileName)
		{
			Rid = new byte[4];
			Name = String.Empty;
			MapNpcs = new List<MapNPC>();
			Unknowns = new List<byte[]>();
			Chests = new List<MapChest>();
			TileRows = new List<TileRow>();
			WarpRows = new List<WarpRow>();
			GfxRows = new List<GFXRow>[GFXLayers];
			Signs = new List<MapSign>();

			for (int layer = 0; layer < GFXLayers; ++layer)
			{
				GfxRows[layer] = new List<GFXRow>();
			}

			int outersize;
			int innersize;

			Packet file = new Packet(File.ReadAllBytes(fileName));
			if (file.Length == 0)
				throw new FileLoadException("The file is empty.");
			file.ReadPos = 0; //use packet wrapper for convenience functions/decoding

			if (file.GetFixedString(3) != "EMF")
				throw new Exception("Corrupt or not an EMF file");

			FileSize = file.Length;

			Rid = file.GetBytes(4);
			byte[] rawname = file.GetBytes(24);

			for (int i = 0; i < 24; ++i)
			{
				if (rawname[i] == 0xFF)
				{
					Array.Resize(ref rawname, i);
					break;
				}
			}

			Name = decodeEmfString(rawname);

			IsPK = file.GetChar() == 3;
			Effect = (MapEffect)file.GetChar();
			Music = file.GetChar();
			MusicExtra = file.GetChar();
			AmbientNoise = file.GetShort();
			Width = file.GetChar();
			Height = file.GetChar();
			FillTile = file.GetShort();
			MapAvailable = file.GetChar();
			CanScroll = file.GetChar();
			RelogX = file.GetChar();
			RelogY = file.GetChar();
			Unknown2 = file.GetChar();

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				MapNpcs.Add(new MapNPC()
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
				Unknowns.Add(file.GetBytes(5));
			}

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				Chests.Add(new MapChest()
				{
					x = file.GetChar(),
					y = file.GetChar(),
					key = file.GetShort(),
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

				TileRow row = new TileRow()
				{
					y = y,
					tiles = new List<Tile>(innersize)
				};

				for (int ii = 0; ii < innersize; ++ii)
				{
					row.tiles.Add(new Tile()
					{
						x = file.GetChar(),
						spec = (TileSpec)file.GetChar()
					});
				}

				TileRows.Add(row);
			}

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				byte y = file.GetChar();
				innersize = file.GetChar();

				WarpRow row = new WarpRow()
				{
					y = y,
					tiles = new List<Warp>(innersize)
				};

				for (int ii = 0; ii < innersize; ++ii)
				{
					row.tiles.Add(new Warp()
					{
						x = file.GetChar(),
						warpMap = file.GetShort(),
						warpX = file.GetChar(),
						warpY = file.GetChar(),
						levelRequirement = file.GetChar(),
						door = file.GetShort()
					});
				}

				WarpRows.Add(row);
			}

			GFXRows = new ESGFXRow[GFXLayers, Height + 1];

			for (int l = 0; l < GFXLayers; l++)
			{
				for (int i = 0; i <= Height; i++)
				{
					GFXRows[l, i].column = new ESGFXColumn[Width + 1];
					for (int ii = 0; ii <= Width; ii++)
					{
						GFXRows[l, i].column[ii].tile = -1;
					}
				}
			}

			for (int layer = 0; layer < GFXLayers; ++layer)
			{
				outersize = file.GetChar();
				GfxRows[layer] = new List<GFXRow>(outersize);

				for (int i = 0; i < outersize; ++i)
				{
					byte y = file.GetChar();
					innersize = file.GetChar();

					GFXRow row = new GFXRow()
					{
						y = y,
						tiles = new List<GFX>(innersize)
					};

					row.tiles = new List<GFX>(innersize);

					for (int ii = 0; ii < innersize; ++ii)
					{
						byte tempx = file.GetChar();
						ushort temptile = (ushort)file.GetShort();
						if (row.y <= Height && tempx <= Width)
						{
							GFXRows[layer, row.y].column[tempx].tile = (int)temptile;
						}
						row.tiles.Add(new GFX()
						{
							x = tempx,
							tile = temptile
						});
					}

					GfxRows[layer].Add(row);
				}
			}

			outersize = file.GetChar();

			for (int i = 0; i < outersize; ++i)
			{
				MapSign sign = new MapSign();

				sign.x = file.GetChar();
				sign.y = file.GetChar();
				int msgLength = file.GetShort() - 1;
				string data = decodeEmfString(file.GetBytes(msgLength));
				int titleLength = file.GetChar();
				sign.title = data.Substring(0, titleLength);
				sign.message = data.Substring(titleLength);

				Signs.Add(sign);
			}
		}
	}
}