// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.IO.Map;

namespace EOLib.IO.Services
{
    public class MapFileLoadService : IMapFileLoadService
    {
        private readonly INumberEncoderService _numberEncoderService;
        private readonly IMapStringEncoderService _mapStringEncoderService;

        public MapFileLoadService(INumberEncoderService numberEncoderService,
                                  IMapStringEncoderService mapStringEncoderService)
        {
            _numberEncoderService = numberEncoderService;
            _mapStringEncoderService = mapStringEncoderService;
        }

        public IMapFile LoadMapByID(int mapID)
        {
            var mapFile = new MapFile(mapID);

            var mapFileBytes = File.ReadAllBytes(string.Format(MapFile.MapFileFormatString, mapID));
            mapFile.DeserializeFromByteArray(mapFileBytes, _numberEncoderService, _mapStringEncoderService);

            return mapFile;
        }

        public IMapFile LoadMapByPath(string pathToMapFile)
        {
            var lastSlash = pathToMapFile.LastIndexOf('\\') < 0 ? pathToMapFile.LastIndexOf('/') : -1;
            if (lastSlash < 0)
                throw new IOException();

            var strID = pathToMapFile.Substring(lastSlash + 1, 5);
            var intID = int.Parse(strID);

            var mapFile = new MapFile(intID);

            var mapFileBytes = File.ReadAllBytes(pathToMapFile);
            mapFile.DeserializeFromByteArray(mapFileBytes, _numberEncoderService, _mapStringEncoderService);

            return mapFile;
        }
    }
}
