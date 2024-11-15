using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;
using EOLib.Shared;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class NPCFileLoadService : BasePubLoadService<ENFRecord>
    {
        protected override string FileFilter => Constants.ENFFilter;

        public NPCFileLoadService(IPubFileDeserializer pubFileDeserializer)
            : base(pubFileDeserializer)
        {
        }

        protected override IPubFile<ENFRecord> Factory() => new ENFFile();
    }
}
