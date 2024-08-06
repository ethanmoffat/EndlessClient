using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class SpellFileLoadService : BasePubLoadService<ESFRecord>
    {
        protected override string FileFilter => PubFileNameConstants.ESFFilter;

        public SpellFileLoadService(IPubFileDeserializer pubFileDeserializer)
            : base(pubFileDeserializer)
        {
        }

        protected override IPubFile<ESFRecord> Factory() => new ESFFile();
    }
}
