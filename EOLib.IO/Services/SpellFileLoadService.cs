using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;
using EOLib.Shared;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class SpellFileLoadService : BasePubLoadService<ESFRecord>
    {
        protected override string FileFilter => Constants.ESFFilter;

        public SpellFileLoadService(IPubFileDeserializer pubFileDeserializer)
            : base(pubFileDeserializer)
        {
        }

        protected override IPubFile<ESFRecord> Factory() => new ESFFile();
    }
}
