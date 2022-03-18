using EOLib.IO.Pub;
using System;

namespace EOLib.IO.Services.Serializers
{
    public interface IPubFileDeserializer
    {
        IPubFile<TRecord> DeserializeFromByteArray<TRecord>(byte[] data, Func<IPubFile<TRecord>> fileFactory)
            where TRecord : class, IPubRecord, new();
    }

    public interface IPubFileSerializer : IPubFileDeserializer
    {
        byte[] SerializeToByteArray<TRecord>(IPubFile<TRecord> file, bool rewriteChecksum = true)
            where TRecord : class, IPubRecord, new();
    }
}
