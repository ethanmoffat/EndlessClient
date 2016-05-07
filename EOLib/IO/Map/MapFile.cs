// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

//****************************************************************************************************
//* This is a derivative of a work licensed under the GPL v3.0
//* Original unmodifeid source is available at:
//*      https://www.assembla.com/code/eo-dev-sharp/subversion/nodes
//*      (see Data/EMF.cs)
//* For additional details on GPL v3.0 see the file GPL3License.txt
//****************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EOLib.Net;

namespace EOLib.IO.Map
{
	public class MapFile : IMapFile
	{
		#region Helper Classes

		private class MapEntityRow<T> : IMapEntityRow<T>
		{
			public int Y { get; set; }
			public List<IMapEntityItem<T>> EntityItems { get; set; }

			public MapEntityRow()
			{
				EntityItems = new List<IMapEntityItem<T>>();
			}
		}

		private class MapEntityItem<T> : IMapEntityItem<T>
		{
			public int X { get; set; }
			public T Value { get; set; }
		}

		#endregion

		public MapFileProperties Properties { get; private set; }

		public IReadOnly2DArray<TileSpec> Tiles { get { return _tiles; } }
		public IReadOnly2DArray<Warp> Warps { get { return _warps; } }
		public IReadOnlyDictionary<MapLayer, IReadOnly2DArray<int>> GFX
		{
			get
			{
				return _gfx.ToDictionary(k => k.Key, v => (IReadOnly2DArray<int>) v.Value);
			}
		}

		public IReadOnlyList<IMapEntityRow<TileSpec>> TileRows { get { return _tileRows; } }
		public IReadOnlyList<IMapEntityRow<Warp>> WarpRows { get { return _warpRows; } }
		public IReadOnlyDictionary<MapLayer, IReadOnlyList<IMapEntityRow<int>>> GFXRows
		{
			get
			{
				return _gfxRows.ToDictionary(k => k.Key, v => (IReadOnlyList<IMapEntityRow<int>>)v.Value);
			}
		}

		public List<NPCSpawn> NPCSpawns { get; private set; }
		public List<byte[]> Unknowns { get; private set; }
		public List<MapChest> Chests { get; private set; }
		public List<MapSign> Signs { get; private set; }

		#region Backing Collections
		//note: the collections must be kept in sync with any set accessors added to the MapFile class

		//backing fields for lookup tables (for rendering)
		private Array2D<TileSpec> _tiles;
		private Array2D<Warp> _warps;
		private Dictionary<MapLayer, Array2D<int>> _gfx;

		//backing fields for Save() function
		private readonly List<MapEntityRow<TileSpec>> _tileRows;
		private readonly List<MapEntityRow<Warp>> _warpRows;
		private readonly Dictionary<MapLayer, List<MapEntityRow<int>>> _gfxRows;

		#endregion

		private readonly IMapStringEncoderService _encoderService;

		public MapFile()
		{
			NPCSpawns = new List<NPCSpawn>();
			Unknowns = new List<byte[]>();
			Chests = new List<MapChest>();
			Signs = new List<MapSign>();

			_tileRows = new List<MapEntityRow<TileSpec>>();
			_warpRows = new List<MapEntityRow<Warp>>();
			_gfxRows = new Dictionary<MapLayer, List<MapEntityRow<int>>>();

			_encoderService = new MapStringEncoderService();
		}

		public void Load(string fileName)
		{
			var lastSlash = fileName.LastIndexOf('\\') < 0 ? fileName.LastIndexOf('/') : -1;
			if (lastSlash < 0)
				throw new IOException();

			var strID = fileName.Substring(lastSlash + 1, 5);
			var intID = int.Parse(strID);

			var filePacket = CreateFilePacketForLoad(fileName);

			SetMapProperties(intID, filePacket);
			ResetCollections();
			ReadNPCSpawns(filePacket);
			ReadUnknowns(filePacket);
			ReadMapChests(filePacket);
			ReadTileSpecs(filePacket);
			ReadWarpTiles(filePacket);
			ReadGFXLayers(filePacket);

			if (filePacket.ReadPosition == filePacket.Length)
				return;

			ReadMapSigns(filePacket);
		}

		public void Save(string fileName)
		{
			IPacketBuilder builder = new PacketBuilder();

			builder = WriteMapProperties(builder);
			builder = WriteNPCSpawns(builder);
			builder = WriteUnknowns(builder);
			builder = WriteMapChests(builder);
			builder = WriteTileSpecs(builder);
			builder = WriteWarpTiles(builder);
			builder = WriteGFXLayers(builder);
			builder = WriteMapSigns(builder);

			var pkt = builder.Build();
			File.WriteAllBytes(fileName, pkt.RawData.ToArray());
		}

		public void RemoveTileAt(int row, int col)
		{
			_tiles[row, col] = TileSpec.None;
			RemoveItemShared(_tileRows, row, col);
		}

		public void RemoveWarpAt(int row, int col)
		{
			_warps[row, col] = null;
			RemoveItemShared(_warpRows, row, col);
		}

		private IPacket CreateFilePacketForLoad(string fileName)
		{
			IPacket filePacket = new Packet(File.ReadAllBytes(fileName));
			if (filePacket.Length == 0)
				throw new FileLoadException("The file is empty.");

			filePacket.Seek(0, SeekOrigin.Begin);
			if (filePacket.ReadString(3) != "EMF")
				throw new IOException("Corrupt or not an EMF file");

			return filePacket;
		}

		private void SetMapProperties(int intID, IPacket filePacket)
		{
			byte[] checkSum;
			Properties = new MapFileProperties()
				.WithMapID(intID)
				.WithFileSize(filePacket.Length)
				.WithChecksum(checkSum = filePacket.ReadBytes(4).ToArray())
				.WithName(_encoderService.DecodeMapString(filePacket.ReadBytes(24).ToArray()))
				.WithPKAvailable(filePacket.ReadChar() == 3 || (checkSum[0] == 0xFF && checkSum[1] == 0x01))
				.WithEffect((MapEffect)filePacket.ReadChar())
				.WithMusic(filePacket.ReadChar())
				.WithMusicExtra(filePacket.ReadChar())
				.WithAmbientNoise(filePacket.ReadShort())
				.WithWidth(filePacket.ReadChar())
				.WithHeight(filePacket.ReadChar())
				.WithFillTile(filePacket.ReadShort())
				.WithMapAvailable(filePacket.ReadChar() == 1)
				.WithScrollAvailable(filePacket.ReadChar() == 1)
				.WithRelogX(filePacket.ReadChar())
				.WithRelogY(filePacket.ReadChar())
				.WithUnknown2(filePacket.ReadChar());
		}

		/// <summary>
		/// Must be called after map properties are set (relies on Width/Height for setting collection capacity)
		/// </summary>
		private void ResetCollections()
		{
			_tileRows.Clear();
			_warpRows.Clear();
			_gfxRows.Clear();

			_tiles = new Array2D<TileSpec>(Properties.Height + 1, Properties.Width + 1, TileSpec.None);
			_warps = new Array2D<Warp>(Properties.Height + 1, Properties.Width + 1, null);
			_gfx = new Dictionary<MapLayer, Array2D<int>>();
			var layers = (MapLayer[]) Enum.GetValues(typeof (MapLayer));
			foreach (var layer in layers)
			{
				_gfx.Add(layer, new Array2D<int>(Properties.Height + 1, Properties.Width + 1, -1));
				_gfxRows.Add(layer, new List<MapEntityRow<int>>(Properties.Height + 1));
			}

			NPCSpawns.Clear();
			Unknowns.Clear();
			Chests.Clear();
			Signs.Clear();
		}

		private void ReadNPCSpawns(IPacket filePacket)
		{
			var numberOfChests = filePacket.ReadChar();
			for (int i = 0; i < numberOfChests; ++i)
			{
				NPCSpawns.Add(new NPCSpawn
				{
					X = filePacket.ReadChar(),
					Y = filePacket.ReadChar(),
					NpcID = filePacket.ReadShort(),
					SpawnType = filePacket.ReadChar(),
					RespawnTime = filePacket.ReadShort(),
					Amount = filePacket.ReadChar()
				});
			}
		}

		private void ReadUnknowns(IPacket filePacket)
		{
			var numberOfUnknowns = filePacket.ReadChar();
			for (int i = 0; i < numberOfUnknowns; ++i)
				Unknowns.Add(filePacket.ReadBytes(4).ToArray());
		}

		private void ReadMapChests(IPacket filePacket)
		{
			var numberOfChests = filePacket.ReadChar();
			for (int i = 0; i < numberOfChests; ++i)
			{
				Chests.Add(new MapChest
				{
					X = filePacket.ReadChar(),
					Y = filePacket.ReadChar(),
					Key = (ChestKey)filePacket.ReadShort(),
					Slot = filePacket.ReadChar(),
					ItemID = filePacket.ReadShort(),
					RespawnTime = filePacket.ReadShort(),
					Amount = filePacket.ReadThree()
				});
			}
		}

		private void ReadTileSpecs(IPacket filePacket)
		{
			var numberOfTileRows = filePacket.ReadChar();
			for (int i = 0; i < numberOfTileRows; ++i)
			{
				var y = filePacket.ReadChar();
				var numberOfTileColumns = filePacket.ReadChar();

				var row = new MapEntityRow<TileSpec> {Y = y};
				for (int j = 0; j < numberOfTileColumns; ++j)
				{
					var x = filePacket.ReadChar();
					var spec = (TileSpec)filePacket.ReadChar();
					if (spec == TileSpec.SpikesTimed)
						Properties = Properties.WithHasTimedSpikes(true);
					_tiles[y, x] = spec;

					row.EntityItems.Add(new MapEntityItem<TileSpec>
					{
						Value = spec,
						X = x
					});
				}

				_tileRows.Add(row);
			}
		}

		private void ReadWarpTiles(IPacket filePacket)
		{
			var numberOfWarpRows = filePacket.ReadChar();
			for (int i = 0; i < numberOfWarpRows; ++i)
			{
				var y = filePacket.ReadChar();
				var numberOfWarpColumns = filePacket.ReadChar();

				var row = new MapEntityRow<Warp> {Y = y};
				for (int j = 0; j < numberOfWarpColumns; ++j)
				{
					var w = new Warp
					{
						Y = y,
						X = filePacket.ReadChar(),
						DestinationMapID = filePacket.ReadShort(),
						DestinationMapX = filePacket.ReadChar(),
						DestinationMapY = filePacket.ReadChar(),
						LevelRequirement = filePacket.ReadChar(),
						DoorType = (DoorSpec)filePacket.ReadShort()
					};

					if (w.Y <= Properties.Height && w.X <= Properties.Width)
					{
						_warps[w.Y, w.X] = w;

						row.EntityItems.Add(new MapEntityItem<Warp>
						{
							Value = w,
							X = w.X
						});
					}
				}

				_warpRows.Add(row);
			}
		}

		private void ReadGFXLayers(IPacket filePacket)
		{
			var layers = Enum.GetValues(typeof(MapLayer));
			foreach (var layerObj in layers)
			{
				var layer = (MapLayer)layerObj;
				if (filePacket.ReadPosition == filePacket.Length)
					break;

				var numberOfRowsThisLayer = filePacket.ReadChar();
				for (int i = 0; i < numberOfRowsThisLayer; ++i)
				{
					var y = filePacket.ReadChar();
					var numberOfColsThisLayer = filePacket.ReadChar();

					var row = new MapEntityRow<int> {Y = y};

					for (int j = 0; j < numberOfColsThisLayer; ++j)
					{
						var x = filePacket.ReadChar();
						var gfxNumber = (ushort)filePacket.ReadShort();
						if (y <= Properties.Height && x <= Properties.Width)
						{
							_gfx[layer][y, x] = gfxNumber;
							row.EntityItems.Add(new MapEntityItem<int>
							{
								Value = gfxNumber,
								X = x
							});
						}
					}

					_gfxRows[layer].Add(row);
				}
			}
		}

		private void ReadMapSigns(IPacket filePacket)
		{
			var numberOfSigns = filePacket.ReadChar();
			for (int i = 0; i < numberOfSigns; ++i)
			{
				MapSign sign = new MapSign { X = filePacket.ReadChar(), Y = filePacket.ReadChar() };

				var msgLength = filePacket.ReadShort() - 1;
				var rawData = filePacket.ReadBytes(msgLength).ToArray();
				var titleLength = filePacket.ReadChar();

				string data = _encoderService.DecodeMapString(rawData);
				sign.Title = data.Substring(0, titleLength);
				sign.Message = data.Substring(titleLength);

				Signs.Add(sign);
			}
		}

		private IPacketBuilder WriteMapProperties(IPacketBuilder filePacket)
		{
			filePacket = filePacket.AddString("EMF")
				.AddBytes(Properties.Checksum);

			var encodedName = _encoderService.EncodeMapString(Properties.Name);
			var paddedName = new byte[24];
			for (int i = paddedName.Length - 1; i >= 0; --i)
				paddedName[i] = 0xFF;
			Array.Copy(encodedName, 0, paddedName, paddedName.Length - encodedName.Length, encodedName.Length);
			
			return filePacket.AddBytes(paddedName)
				.AddChar(Properties.PKAvailable ? (byte)3 : (byte)0)
				.AddChar((byte)Properties.Effect)
				.AddChar(Properties.Music)
				.AddChar(Properties.MusicExtra)
				.AddShort(Properties.AmbientNoise)
				.AddChar(Properties.Width)
				.AddChar(Properties.Height)
				.AddShort(Properties.FillTile)
				.AddChar(Properties.MapAvailable ? (byte)1 : (byte)0)
				.AddChar(Properties.CanScroll ? (byte)1 : (byte)0)
				.AddChar(Properties.RelogX)
				.AddChar(Properties.RelogY)
				.AddChar(Properties.Unknown2);
		}

		private IPacketBuilder WriteNPCSpawns(IPacketBuilder filePacket)
		{
			filePacket = filePacket.AddChar((byte)NPCSpawns.Count);
			foreach (var spawn in NPCSpawns)
			{
				filePacket = filePacket.AddChar(spawn.X)
					.AddChar(spawn.Y)
					.AddShort(spawn.NpcID)
					.AddChar(spawn.SpawnType)
					.AddShort(spawn.RespawnTime)
					.AddChar(spawn.Amount);
			}
			return filePacket;
		}

		private IPacketBuilder WriteUnknowns(IPacketBuilder filePacket)
		{
			filePacket = filePacket.AddChar((byte)Unknowns.Count);
			foreach (var b in Unknowns)
				filePacket = filePacket.AddBytes(b);
			return filePacket;
		}

		private IPacketBuilder WriteMapChests(IPacketBuilder filePacket)
		{
			filePacket = filePacket.AddChar((byte)Chests.Count);
			foreach (MapChest chest in Chests)
			{
				filePacket = filePacket.AddChar(chest.X)
					.AddChar(chest.Y)
					.AddShort((short) chest.Key)
					.AddChar(chest.Slot)
					.AddShort(chest.ItemID)
					.AddShort(chest.RespawnTime)
					.AddThree(chest.Amount);
			}
			return filePacket;
		}

		private IPacketBuilder WriteTileSpecs(IPacketBuilder filePacket)
		{
			filePacket = filePacket.AddChar((byte)_tileRows.Count);
			foreach (var tr in _tileRows)
			{
				filePacket = filePacket.AddChar((byte)tr.Y);
				filePacket = filePacket.AddChar((byte)tr.EntityItems.Count);
				foreach (var tt in tr.EntityItems)
				{
					filePacket = filePacket.AddChar((byte)tt.X);
					filePacket = filePacket.AddChar((byte)tt.Value);
				}
			}
			return filePacket;
		}

		private IPacketBuilder WriteWarpTiles(IPacketBuilder filePacket)
		{
			filePacket = filePacket.AddChar((byte)_warpRows.Count);

			foreach (var wr in _warpRows)
			{
				filePacket = filePacket.AddChar((byte)wr.Y)
					.AddChar((byte)wr.EntityItems.Count);

				foreach (var warpEntity in wr.EntityItems)
				{
					var ww = warpEntity.Value;
					filePacket = filePacket.AddChar(ww.X)
						.AddShort(ww.DestinationMapID)
						.AddChar(ww.DestinationMapX)
						.AddChar(ww.DestinationMapY)
						.AddChar(ww.LevelRequirement)
						.AddShort((short) ww.DoorType);
				}
			}

			return filePacket;
		}

		private IPacketBuilder WriteGFXLayers(IPacketBuilder filePacket)
		{
			var layers = (MapLayer[])Enum.GetValues(typeof(MapLayer));

			foreach (var layer in layers)
			{
				filePacket = filePacket.AddChar((byte)_gfxRows[layer].Count);
				foreach (var row in _gfxRows[layer])
				{
					filePacket = filePacket.AddChar((byte)row.Y);
					filePacket = filePacket.AddChar((byte)row.EntityItems.Count);
					foreach (var gfx in row.EntityItems)
					{
						filePacket = filePacket.AddChar((byte)gfx.X);
						filePacket = filePacket.AddShort((short)gfx.Value);
					}
				}
			}

			return filePacket;
		}

		private IPacketBuilder WriteMapSigns(IPacketBuilder filePacket)
		{
			filePacket = filePacket.AddChar((byte)Signs.Count);

			foreach (var sign in Signs)
			{
				filePacket = filePacket.AddChar(sign.X)
					.AddChar(sign.Y)
					.AddShort((short)(sign.Message.Length + sign.Title.Length + 1));

				var fileMsg = new byte[sign.Message.Length + sign.Title.Length];
				var rawTitle = _encoderService.EncodeMapString(sign.Title);
				var rawMessage = _encoderService.EncodeMapString(sign.Message);
				Array.Copy(rawTitle, fileMsg, fileMsg.Length);
				Array.Copy(rawMessage, 0, fileMsg, rawTitle.Length, rawMessage.Length);
				filePacket = filePacket.AddBytes(fileMsg);
				filePacket = filePacket.AddChar((byte)rawTitle.Length);
			}

			return filePacket;
		}

		private static void RemoveItemShared<T>(IList<MapEntityRow<T>> collection, int row, int col)
		{
			IMapEntityItem<T> tile = null;
			var tileRow = collection.SingleOrDefault(tr => tr.Y == row &&
														   (tile = tr.EntityItems.SingleOrDefault(tt => tt.X == col)) != null);
			if (tileRow == null || tile == null)
				return;
			tileRow.EntityItems.Remove(tile);
			if (!tileRow.EntityItems.Any())
				collection.Remove(tileRow);
		}

		public static IMapFile FromBytes(int mapID, IEnumerable<byte> bytes)
		{
			var filePacket = new Packet(bytes.ToArray());
			filePacket.Seek(0, SeekOrigin.Begin);
			if (filePacket.ReadString(3) != "EMF")
				throw new IOException("Corrupt or not an EMF file");
			
			var mapFile = new MapFile();
			mapFile.SetMapProperties(mapID, filePacket);
			mapFile.ResetCollections();
			mapFile.ReadNPCSpawns(filePacket);
			mapFile.ReadUnknowns(filePacket);
			mapFile.ReadMapChests(filePacket);
			mapFile.ReadTileSpecs(filePacket);
			mapFile.ReadWarpTiles(filePacket);
			mapFile.ReadGFXLayers(filePacket);

			if (filePacket.ReadPosition != filePacket.Length)
				mapFile.ReadMapSigns(filePacket);

			return mapFile;
		}
	}
}