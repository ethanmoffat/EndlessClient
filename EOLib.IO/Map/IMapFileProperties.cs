// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public interface IMapFileProperties
    {
        string FileType { get; }
        int MapID { get; }
        int FileSize { get; }

        byte[] Checksum { get; }
        string Name { get; }
        byte Width { get; }
        byte Height { get; }
        MapEffect Effect { get; }
        byte Music { get; }
        byte MusicExtra { get; }
        short AmbientNoise { get; }
        short FillTile { get; }
        byte RelogX { get; }
        byte RelogY { get; }
        byte Unknown2 { get; }
        bool MapAvailable { get; }
        bool CanScroll { get; }
        bool PKAvailable { get; }
        bool HasTimedSpikes { get; }

        IMapFileProperties WithMapID(int id);
        IMapFileProperties WithFileSize(int fileSize);

        IMapFileProperties WithChecksum(byte[] checksum);
        IMapFileProperties WithName(string name);
        IMapFileProperties WithWidth(byte width);
        IMapFileProperties WithHeight(byte height);
        IMapFileProperties WithEffect(MapEffect effect);
        IMapFileProperties WithMusic(byte music);
        IMapFileProperties WithMusicExtra(byte musicExtra);
        IMapFileProperties WithAmbientNoise(short ambientNoise);
        IMapFileProperties WithFillTile(short fillTile);
        IMapFileProperties WithRelogX(byte relogX);
        IMapFileProperties WithRelogY(byte relogY);
        IMapFileProperties WithUnknown2(byte unknown2);
        IMapFileProperties WithMapAvailable(bool isAvailable);
        IMapFileProperties WithScrollAvailable(bool canScroll);
        IMapFileProperties WithPKAvailable(bool pkAvailable);
        IMapFileProperties WithHasTimedSpikes(bool hasTimedSpikes);
    }
}