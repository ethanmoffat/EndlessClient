using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    public interface IMapDeserializer<T>
    {
        T DeserializeFromByteArray(byte[] data);
    }

    public interface IMapFileSerializer : IMapDeserializer<IMapFile>
    {
        byte[] SerializeToByteArray(IMapFile mapEntity, bool rewriteChecksum = true);
    }

    public interface IMapEntitySerializer<T> : IMapDeserializer<T>
    {
        byte[] SerializeToByteArray(T mapEntity);
    }
}