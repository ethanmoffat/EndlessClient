using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;
using EOLib.Shared;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class ClassFileLoadService : BasePubLoadService<ECFRecord>
    {
        protected override string FileFilter => Constants.ECFFilter;

        public ClassFileLoadService(IPubFileDeserializer pubFileDeserializer)
            : base(pubFileDeserializer)
        {
        }

        protected override IPubFile<ECFRecord> Factory() => new ECFFile();
    }
}
