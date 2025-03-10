﻿using System;
using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Map;
using EOLib.IO.Services.Serializers;
using EOLib.Shared;

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
            var directoryName = Constants.MapDirectory;
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            File.WriteAllBytes(string.Format(Constants.MapFileFormatString, mapFile.Properties.MapID),
                               _mapFileSerializer.SerializeToByteArray(mapFile, rewriteChecksum));
        }

        public void SaveFile(string path, IMapFile mapFile, bool rewriteChecksum = true)
        {
            if (!path.ToLower().EndsWith(".emf"))
                throw new ArgumentException("Must specify an emf file", nameof(path));

            File.WriteAllBytes(path, _mapFileSerializer.SerializeToByteArray(mapFile, rewriteChecksum));
        }
    }
}
