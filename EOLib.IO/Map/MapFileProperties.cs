// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.IO.Map
{
    public class MapFileProperties : IMapFileProperties
    {
        public const int DATA_SIZE = 46;

        public string FileType { get { return "EMF"; } }
        public int MapID { get; private set; }
        public int FileSize { get; private set; }

        public byte[] Checksum { get; private set; }
        public string Name { get; private set; }

        public byte Width { get; private set; }
        public byte Height { get; private set; }

        public MapEffect Effect { get; private set; }

        public byte Music { get; private set; }
        public byte MusicExtra { get; private set; }
        public short AmbientNoise { get; private set; }

        public short FillTile { get; private set; }
        public byte RelogX { get; private set; }
        public byte RelogY { get; private set; }
        public byte Unknown2 { get; private set; }

        public bool MapAvailable { get; private set; }
        public bool CanScroll { get; private set; }
        public bool PKAvailable { get; private set; }

        public bool HasTimedSpikes { get; private set; }

        public MapFileProperties()
        {
            Name = "";
            Checksum = new byte[4];
        }

        public IMapFileProperties WithMapID(int id)
        {
            var clone = Clone();
            clone.MapID = id;
            return clone;
        }

        public IMapFileProperties WithFileSize(int fileSize)
        {
            var clone = Clone();
            clone.FileSize = fileSize;
            return clone;
        }

        public IMapFileProperties WithChecksum(byte[] checksum)
        {
            var clone = Clone();
            clone.Checksum = checksum;
            return clone;
        }

        public IMapFileProperties WithName(string name)
        {
            var clone = Clone();
            clone.Name = name;
            return clone;
        }

        public IMapFileProperties WithWidth(byte width)
        {
            var clone = Clone();
            clone.Width = width;
            return clone;
        }

        public IMapFileProperties WithHeight(byte height)
        {
            var clone = Clone();
            clone.Height = height;
            return clone;
        }

        public IMapFileProperties WithEffect(MapEffect effect)
        {
            var clone = Clone();
            clone.Effect = effect;
            return clone;
        }

        public IMapFileProperties WithMusic(byte music)
        {
            var clone = Clone();
            clone.Music = music;
            return clone;
        }

        public IMapFileProperties WithMusicExtra(byte musicExtra)
        {
            var clone = Clone();
            clone.MusicExtra = musicExtra;
            return clone;
        }

        public IMapFileProperties WithAmbientNoise(short ambientNoise)
        {
            var clone = Clone();
            clone.AmbientNoise = ambientNoise;
            return clone;
        }

        public IMapFileProperties WithFillTile(short fillTile)
        {
            var clone = Clone();
            clone.FillTile = fillTile;
            return clone;
        }

        public IMapFileProperties WithRelogX(byte relogX)
        {
            var clone = Clone();
            clone.RelogX = relogX;
            return clone;
        }

        public IMapFileProperties WithRelogY(byte relogY)
        {
            var clone = Clone();
            clone.RelogY = relogY;
            return clone;
        }

        public IMapFileProperties WithUnknown2(byte unknown2)
        {
            var clone = Clone();
            clone.Unknown2 = unknown2;
            return clone;
        }

        public IMapFileProperties WithMapAvailable(bool isAvailable)
        {
            var clone = Clone();
            clone.MapAvailable = isAvailable;
            return clone;
        }

        public IMapFileProperties WithScrollAvailable(bool canScroll)
        {
            var clone = Clone();
            clone.CanScroll = canScroll;
            return clone;
        }

        public IMapFileProperties WithPKAvailable(bool pkAvailable)
        {
            var clone = Clone();
            clone.PKAvailable = pkAvailable;
            return clone;
        }

        public IMapFileProperties WithHasTimedSpikes(bool hasTimedSpikes)
        {
            var clone = Clone();
            clone.HasTimedSpikes = hasTimedSpikes;
            return clone;
        }

        private MapFileProperties Clone()
        {
            var props = new MapFileProperties
            {
                MapID = MapID,
                FileSize = FileSize,
                Checksum = new byte[Checksum.Length],
                Name = Name,
                Width = Width,
                Height = Height,
                Effect = Effect,
                Music = Music,
                MusicExtra = MusicExtra,
                AmbientNoise = AmbientNoise,
                FillTile = FillTile,
                RelogX = RelogX,
                RelogY = RelogY,
                Unknown2 = Unknown2,
                MapAvailable = MapAvailable,
                CanScroll = CanScroll,
                PKAvailable = PKAvailable,
                HasTimedSpikes = HasTimedSpikes
            };
            Array.Copy(Checksum, props.Checksum, props.Checksum.Length);

            return props;
        }
    }
}

