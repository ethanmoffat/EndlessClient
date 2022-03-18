using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubLoadService<ECFRecord>))]
    public class ClassFileLoadService : IPubLoadService<ECFRecord>
    {
        private readonly IPubFileDeserializer _pubFileDeserializer;

        public ClassFileLoadService(IPubFileDeserializer pubFileDeserializer)
        {
            _pubFileDeserializer = pubFileDeserializer;
        }

        public IPubFile<ECFRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PathToECFFile);
        }

        public IPubFile<ECFRecord> LoadPubFromExplicitFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            return _pubFileDeserializer.DeserializeFromByteArray<ECFRecord>(fileBytes, () => new ECFFile());
        }
    }
}
