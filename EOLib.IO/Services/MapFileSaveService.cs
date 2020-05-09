using System;
using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Map;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IMapFileSaveService))]
    public class MapFileSaveService : IMapFileSaveService
    {
        private readonly ISerializer<IMapFile> _mapFileSerializer;

        public MapFileSaveService(ISerializer<IMapFile> mapFileSerializer)
        {
            _mapFileSerializer = mapFileSerializer;
        }

        public void SaveFileToDefaultDirectory(IMapFile mapFile)
        {
            var directoryName = Path.GetDirectoryName(string.Format(MapFile.MapFileFormatString, 1)) ?? "";
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            File.WriteAllBytes(string.Format(MapFile.MapFileFormatString, mapFile.Properties.MapID),
                               _mapFileSerializer.SerializeToByteArray(mapFile));
        }

        public void SaveFile(string path, IMapFile mapFile)
        {
            if (!path.ToLower().EndsWith(".emf"))
                throw new ArgumentException("Must specify an emf file", nameof(path));

            File.WriteAllBytes(path, _mapFileSerializer.SerializeToByteArray(mapFile));
        }
    }
}