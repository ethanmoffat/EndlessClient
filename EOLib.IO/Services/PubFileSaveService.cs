using System.IO;
using AutomaticTypeMapper;
using EOLib.IO.Pub;
using EOLib.IO.Services.Serializers;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IPubFileSaveService))]
    public class PubFileSaveService : IPubFileSaveService
    {
        private readonly IPubFileSerializer _pubFileSerializer;

        public PubFileSaveService(IPubFileSerializer pubFileSerializer)
        {
            _pubFileSerializer = pubFileSerializer;
        }

        public void SaveFile<TRecord>(string path, IPubFile<TRecord> pubFile, bool rewriteChecksum = true)
            where TRecord : class, IPubRecord, new()
        {
            var directoryName = Path.GetDirectoryName(path) ?? "";
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var pubFileBytes = _pubFileSerializer.SerializeToByteArray<TRecord>(pubFile, rewriteChecksum);
            File.WriteAllBytes(path, pubFileBytes);
        }
    }
}