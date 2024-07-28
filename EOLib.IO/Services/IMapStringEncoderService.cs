namespace EOLib.IO.Services
{
    public interface IMapStringEncoderService
    {
        string DecodeMapString(byte[] chars);

        byte[] EncodeMapString(string s, int length);
    }
}