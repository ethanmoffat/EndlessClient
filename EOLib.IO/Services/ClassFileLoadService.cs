using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class ClassFileLoadService : BasePubLoadService<ECFRecord>
    {
        protected override string FileFilter => PubFileNameConstants.ECFFilter;

        public ClassFileLoadService(IPubFileDeserializer pubFileDeserializer)
            : base(pubFileDeserializer)
        {
        }

        protected override IPubFile<ECFRecord> Factory() => new ECFFile();
    }
}
