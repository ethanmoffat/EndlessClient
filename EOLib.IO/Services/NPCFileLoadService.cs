// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubLoadService<ENFRecord>))]
    public class NPCFileLoadService : IPubLoadService<ENFRecord>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public NPCFileLoadService(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public IPubFile<ENFRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PathToENFFile);
        }

        public IPubFile<ENFRecord> LoadPubFromExplicitFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            
            var pub = new ENFFile();
            pub.DeserializeFromByteArray(fileBytes, _numberEncoderService);

            return pub;
        }
    }
}
