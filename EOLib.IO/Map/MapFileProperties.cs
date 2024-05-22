using System;
using System.Collections.Generic;

namespace EOLib.IO.Map
{
    public class MapFileProperties : IMapFileProperties
    {
        public const int DATA_SIZE = 46;

        public string FileType => "EMF";
        public int MapID { get; private set; }
        public int FileSize { get; private set; }

        public IReadOnlyList<int> Checksum { get; private set; }
        public string Name { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public MapEffect Effect { get; private set; }

        public int Music { get; private set; }
        public MusicControl Control { get; private set; }
        public int AmbientNoise { get; private set; }

        public int FillTile { get; private set; }
        public int RelogX { get; private set; }
        public int RelogY { get; private set; }
        public int Unknown2 { get; private set; }

        public bool MapAvailable { get; private set; }
        public bool CanScroll { get; private set; }
        public bool PKAvailable { get; private set; }

        public bool HasTimedSpikes { get; private set; }

        public MapFileProperties()
        {
            Name = "";
            Checksum = new List<int> { 0, 0 };
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

        public IMapFileProperties WithChecksum(IReadOnlyList<int> checksum)
        {
            if (checksum.Count != 2)
                throw new ArgumentException("Checksum should be 2 eo 'short' values", nameof(checksum));

            var clone = Clone();
            clone.Checksum = new List<int>(checksum);
            return clone;
        }

        public IMapFileProperties WithName(string name)
        {
            var clone = Clone();
            clone.Name = name;
            return clone;
        }

        public IMapFileProperties WithWidth(int width)
        {
            var clone = Clone();
            clone.Width = width;
            return clone;
        }

        public IMapFileProperties WithHeight(int height)
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

        public IMapFileProperties WithMusic(int music)
        {
            var clone = Clone();
            clone.Music = music;
            return clone;
        }

        public IMapFileProperties WithControl(MusicControl control)
        {
            var clone = Clone();
            clone.Control = control;
            return clone;
        }

        public IMapFileProperties WithAmbientNoise(int ambientNoise)
        {
            var clone = Clone();
            clone.AmbientNoise = ambientNoise;
            return clone;
        }

        public IMapFileProperties WithFillTile(int fillTile)
        {
            var clone = Clone();
            clone.FillTile = fillTile;
            return clone;
        }

        public IMapFileProperties WithRelogX(int relogX)
        {
            var clone = Clone();
            clone.RelogX = relogX;
            return clone;
        }

        public IMapFileProperties WithRelogY(int relogY)
        {
            var clone = Clone();
            clone.RelogY = relogY;
            return clone;
        }

        public IMapFileProperties WithUnknown2(int unknown2)
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
            return new MapFileProperties
            {
                MapID = MapID,
                FileSize = FileSize,
                Checksum = new List<int>(Checksum),
                Name = Name,
                Width = Width,
                Height = Height,
                Effect = Effect,
                Music = Music,
                Control = Control,
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
        }
    }
}

