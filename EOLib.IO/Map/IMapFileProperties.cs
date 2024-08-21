using System.Collections.Generic;

namespace EOLib.IO.Map
{
    public interface IMapFileProperties
    {
        string FileType { get; }
        int MapID { get; }
        int FileSize { get; }

        IReadOnlyList<int> Checksum { get; }
        string Name { get; }
        int Width { get; }
        int Height { get; }
        MapEffect Effect { get; }
        int Music { get; }
        MusicControl Control { get; }
        int AmbientNoise { get; }
        int FillTile { get; }
        int RelogX { get; }
        int RelogY { get; }
        int Unknown2 { get; }
        bool MapAvailable { get; }
        bool CanScroll { get; }
        bool PKAvailable { get; }
        bool HasTimedSpikes { get; }

        IMapFileProperties WithMapID(int id);
        IMapFileProperties WithFileSize(int fileSize);

        IMapFileProperties WithChecksum(IReadOnlyList<int> checksum);
        IMapFileProperties WithName(string name);
        IMapFileProperties WithWidth(int width);
        IMapFileProperties WithHeight(int height);
        IMapFileProperties WithEffect(MapEffect effect);
        IMapFileProperties WithMusic(int music);
        IMapFileProperties WithControl(MusicControl control);
        IMapFileProperties WithAmbientNoise(int ambientNoise);
        IMapFileProperties WithFillTile(int fillTile);
        IMapFileProperties WithRelogX(int relogX);
        IMapFileProperties WithRelogY(int relogY);
        IMapFileProperties WithUnknown2(int unknown2);
        IMapFileProperties WithMapAvailable(bool isAvailable);
        IMapFileProperties WithScrollAvailable(bool canScroll);
        IMapFileProperties WithPKAvailable(bool pkAvailable);
        IMapFileProperties WithHasTimedSpikes(bool hasTimedSpikes);
    }
}
