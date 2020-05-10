using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubLoadService<ESFRecord>))]
    public class SpellFileLoadService : IPubLoadService<ESFRecord>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public SpellFileLoadService(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public IPubFile<ESFRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PathToESFFile);
        }

        public IPubFile<ESFRecord> LoadPubFromExplicitFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            
            var pub = new ESFFile();
            pub.DeserializeFromByteArray(fileBytes, _numberEncoderService);

            return pub;
        }
    }
}
