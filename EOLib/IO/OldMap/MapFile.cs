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
using EOLib.IO.Map;
using EOLib.IO.Services;
using EOLib.Net;

namespace EOLib.IO.OldMap
{
    public class MapFile : IMapFile
    {
        public IMapFileProperties Properties { get; private set; }

        public IReadOnly2DArray<TileSpec> Tiles { get { return _tiles; } }
        public IReadOnly2DArray<Warp> Warps { get { return _warps; } }
        public IReadOnlyDictionary<MapLayer, IReadOnly2DArray<int>> GFX
        {
            get
            {
                return _gfx.ToDictionary(k => k.Key, v => (IReadOnly2DArray<int>) v.Value);
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

        #endregion

        private readonly IMapStringEncoderService _encoderService;

        public MapFile()
        {
            NPCSpawns = new List<NPCSpawn>();
            Unknowns = new List<byte[]>();
            Chests = new List<MapChest>();
            Signs = new List<MapSign>();

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
            Properties = new MapFileProperties();

            Properties = Properties
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
            _tiles = new Array2D<TileSpec>(Properties.Height + 1, Properties.Width + 1, TileSpec.None);
            _warps = new Array2D<Warp>(Properties.Height + 1, Properties.Width + 1, null);
            _gfx = new Dictionary<MapLayer, Array2D<int>>();
            var layers = (MapLayer[]) Enum.GetValues(typeof (MapLayer));
            foreach (var layer in layers)
            {
                _gfx.Add(layer, new Array2D<int>(Properties.Height + 1, Properties.Width + 1, -1));
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

                for (int j = 0; j < numberOfTileColumns; ++j)
                {
                    var x = filePacket.ReadChar();
                    var spec = (TileSpec)filePacket.ReadChar();
                    if (spec == TileSpec.SpikesTimed)
                        Properties = Properties.WithHasTimedSpikes(true);
                    _tiles[y, x] = spec;
                }
            }
        }

        private void ReadWarpTiles(IPacket filePacket)
        {
            var numberOfWarpRows = filePacket.ReadChar();
            for (int i = 0; i < numberOfWarpRows; ++i)
            {
                var y = filePacket.ReadChar();
                var numberOfWarpColumns = filePacket.ReadChar();

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
                    }
                }
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

                    for (int j = 0; j < numberOfColsThisLayer; ++j)
                    {
                        var x = filePacket.ReadChar();
                        var gfxNumber = (ushort)filePacket.ReadShort();
                        if (y <= Properties.Height && x <= Properties.Width)
                        {
                            _gfx[layer][y, x] = gfxNumber;
                        }
                    }
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
    }
}