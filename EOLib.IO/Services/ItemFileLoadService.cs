// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubLoadService<EIFRecord>))]
    public class ItemFileLoadService : IPubLoadService<EIFRecord>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public ItemFileLoadService(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public IPubFile<EIFRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PathToEIFFile);
        }

        public IPubFile<EIFRecord> LoadPubFromExplicitFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            
            var pub = new EIFFile();
            pub.DeserializeFromByteArray(fileBytes, _numberEncoderService);

            return pub;
        }
    }
}
