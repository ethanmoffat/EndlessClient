using EOLib.IO.Map;

namespace EOLib.IO.Services
{
    public interface IMapFileLoadService
    {
        IMapFile LoadMapByID(int mapID);

        IMapFile LoadMapByPath(string pathToMapFile);
    }
}
