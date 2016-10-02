// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.IO.Map;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    public class MapFileLoadService : IMapFileLoadService
    {
        private readonly ISerializer<IMapFile> _mapFileSerializer;

        public MapFileLoadService(ISerializer<IMapFile> mapFileSerializer)
        {
            _mapFileSerializer = mapFileSerializer;
        }

        public IMapFile LoadMapByID(int mapID)
        {
            var mapFileBytes = File.ReadAllBytes(string.Format(MapFile.MapFileFormatString, mapID));

            var mapFile = _mapFileSerializer
                .DeserializeFromByteArray(mapFileBytes)
                .WithMapID(mapID);

            return mapFile;
        }

        public IMapFile LoadMapByPath(string pathToMapFile)
        {
            var intID = new MapPathToIDConverter().ConvertFromPathToID(pathToMapFile);
            var mapFileBytes = File.ReadAllBytes(pathToMapFile);

            var mapFile = _mapFileSerializer
                .DeserializeFromByteArray(mapFileBytes)
                .WithMapID(intID);

            return mapFile;
        }
    }
}
