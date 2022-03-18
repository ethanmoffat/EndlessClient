using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubLoadService<ENFRecord>))]
    public class NPCFileLoadService : IPubLoadService<ENFRecord>
    {
        private readonly IPubFileDeserializer _pubFileDeserializer;

        public NPCFileLoadService(IPubFileDeserializer pubFileDeserializer)
        {
            _pubFileDeserializer = pubFileDeserializer;
        }

        public IPubFile<ENFRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(PubFileNameConstants.PathToENFFile);
        }

        public IPubFile<ENFRecord> LoadPubFromExplicitFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            return _pubFileDeserializer.DeserializeFromByteArray(fileBytes, () => new ENFFile());
        }
    }
}
