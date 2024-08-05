using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class NPCFileLoadService : BasePubLoadService<ENFRecord>
    {
        protected override string FileFilter => PubFileNameConstants.ENFFilter;

        public NPCFileLoadService(IPubFileDeserializer pubFileDeserializer)
            : base(pubFileDeserializer)
        {
        }

        protected override IPubFile<ENFRecord> Factory() => new ENFFile();
    }
}