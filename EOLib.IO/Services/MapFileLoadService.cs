using System;
using System.IO;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using EOLib.IO.Map;
using EOLib.IO.Services.Serializers;

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
            var mapFileBytes = File.ReadAllBytes(GetPath(string.Format(MapFile.MapFileFormatString, mapID)));

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

        private string GetPath(string inputPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                return Path.Combine(home, ".endlessclient", inputPath);
            }
            else
            {
                return inputPath;
            }
        }
    }
}
