namespace EOLib.IO.Services
{
    public interface INumberEncoderService
    {
        byte[] EncodeNumber(int number, int size);

        int DecodeNumber(params byte[] b);
    }
}
