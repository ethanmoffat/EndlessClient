﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using EOLib.IO.Map;
using EOLib.IO.Services.Serializers;
using EOLib.Shared;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IMapFileLoadService))]
    public class MapFileLoadService : IMapFileLoadService
    {
        private readonly IMapDeserializer<IMapFile> _mapFileSerializer;

        public MapFileLoadService(IMapDeserializer<IMapFile> mapFileSerializer)
        {
            _mapFileSerializer = mapFileSerializer;
        }

        public IMapFile LoadMapByID(int mapID)
        {
            var mapFileBytes = File.ReadAllBytes(string.Format(Constants.MapFileFormatString, mapID));

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
