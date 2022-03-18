using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubLoadService<ESFRecord>))]
    public class SpellFileLoadService : IPubLoadService<ESFRecord>
    {
        private readonly IPubFileDeserializer _pubFileDeserializer;

        public SpellFileLoadService(IPubFileDeserializer pubFileDeserializer)
        {
            _pubFileDeserializer = pubFileDeserializer;
        }

        public IPubFile<ESFRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PathToESFFile);
        }

        public IPubFile<ESFRecord> LoadPubFromExplicitFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            return _pubFileDeserializer.DeserializeFromByteArray(fileBytes, () => new ESFFile());
        }
    }
}
