// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Map;

namespace EOLib.IO.Services
{
    public class MapFileLoadService : IMapFileLoadService
    {
        public IMapFile LoadMapByID(int mapID)
        {
            return LoadMapByPath(string.Format(Constants.MapFileFormatString, mapID));
        }

        public IMapFile LoadMapByPath(string pathToMapFile)
        {
            var mapFile = new MapFile();
            mapFile.Load(pathToMapFile);
            return mapFile;
        }
    }
}
