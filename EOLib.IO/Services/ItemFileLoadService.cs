using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubLoadService<EIFRecord>))]
    public class ItemFileLoadService : IPubLoadService<EIFRecord>
    {
        private readonly IPubFileDeserializer _pubFileDeserializer;

        public ItemFileLoadService(IPubFileDeserializer pubFileDeserializer)
        {
            _pubFileDeserializer = pubFileDeserializer;
        }

        public IPubFile<EIFRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PathToEIFFile);
        }

        public IPubFile<EIFRecord> LoadPubFromExplicitFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            return _pubFileDeserializer.DeserializeFromByteArray(fileBytes, () => new EIFFile());
        }
    }
}
