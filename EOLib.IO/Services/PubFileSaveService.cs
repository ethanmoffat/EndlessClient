// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubFileSaveService))]
    public class PubFileSaveService : IPubFileSaveService
    {
        private readonly INumberEncoderService _numberEncoderService;

        public PubFileSaveService(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public void SaveFile(string path, IPubFile pubFile)
        {
            var directoryName = Path.GetDirectoryName(path) ?? "";
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var pubFileBytes = pubFile.SerializeToByteArray(_numberEncoderService);
            File.WriteAllBytes(path, pubFileBytes);
        }
    }
}