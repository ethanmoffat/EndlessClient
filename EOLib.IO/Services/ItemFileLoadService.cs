using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class ItemFileLoadService : BasePubLoadService<EIFRecord>
    {
        protected override string FileFilter => PubFileNameConstants.EIFFilter;

        public ItemFileLoadService(IPubFileDeserializer pubFileDeserializer)
            : base(pubFileDeserializer)
        {
        }

        protected override IPubFile<EIFRecord> Factory() => new EIFFile();
    }
}
