using AutomaticTypeMapper;
using EOLib.IO.Pub;

namespace EOLib.IO.Repositories
{
    [MappedType(BaseType = typeof(IPubFileRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IPubFileProvider), IsSingleton = true)]
    [MappedType(BaseType = typeof(IEIFFileRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IEIFFileProvider), IsSingleton = true)]
    [MappedType(BaseType = typeof(IENFFileRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IENFFileProvider), IsSingleton = true)]
    [MappedType(BaseType = typeof(IESFFileRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IESFFileProvider), IsSingleton = true)]
    [MappedType(BaseType = typeof(IECFFileRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IECFFileProvider), IsSingleton = true)]
    public class PubFileRepository : IPubFileRepository, IPubFileProvider
    {
        public IPubFile<EIFRecord> EIFFile { get; set; }

        public IPubFile<ENFRecord> ENFFile { get; set; }

        public IPubFile<ESFRecord> ESFFile { get; set; }

        public IPubFile<ECFRecord> ECFFile { get; set; }
    }
}