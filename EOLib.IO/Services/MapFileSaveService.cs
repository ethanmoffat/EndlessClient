using System;
using System.IO;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using EOLib.IO.Map;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IMapFileSaveService))]
    public class MapFileSaveService : IMapFileSaveService
    {
        private readonly IMapFileSerializer _mapFileSerializer;

        public MapFileSaveService(IMapFileSerializer mapFileSerializer)
        {
            _mapFileSerializer = mapFileSerializer;
        }

        public void SaveFileToDefaultDirectory(IMapFile mapFile, bool rewriteChecksum = true)
        {
            var directoryName = GetPath(Path.GetDirectoryName(string.Format(MapFile.MapFileFormatString, 1)) ?? "");
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            File.WriteAllBytes(GetPath(string.Format(MapFile.MapFileFormatString, mapFile.Properties.MapID)),
                               _mapFileSerializer.SerializeToByteArray(mapFile, rewriteChecksum));
        }

        public void SaveFile(string path, IMapFile mapFile, bool rewriteChecksum = true)
        {
            if (!path.ToLower().EndsWith(".emf"))
                throw new ArgumentException("Must specify an emf file", nameof(path));

            File.WriteAllBytes(path, _mapFileSerializer.SerializeToByteArray(mapFile, rewriteChecksum));
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
