namespace EOLib.IO.Services.Serializers
{
    public interface ISerializer<T>
    {
        byte[] SerializeToByteArray(T mapEntity);

        T DeserializeFromByteArray(byte[] data);
    }
}
