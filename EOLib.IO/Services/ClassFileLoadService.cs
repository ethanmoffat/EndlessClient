// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubLoadService<ECFRecord>))]
    public class ClassFileLoadService : IPubLoadService<ECFRecord>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public ClassFileLoadService(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public IPubFile<ECFRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PathToECFFile);
        }

        public IPubFile<ECFRecord> LoadPubFromExplicitFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            
            var pub = new ECFFile();
            pub.DeserializeFromByteArray(fileBytes, _numberEncoderService);

            return pub;
        }
    }
}
