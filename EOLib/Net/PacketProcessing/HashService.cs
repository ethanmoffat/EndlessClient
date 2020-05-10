using AutomaticTypeMapper;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType]
    public class HashService : IHashService
    {
        public int StupidHash(int seed)
        {
            ++seed;
            return 110905 + (seed % 9 + 1) * ((11092004 - seed) % ((seed % 11 + 1) * 119)) * 119 + seed % 2004;
        }
    }

    public interface IHashService
    {
        int StupidHash(int seed);
    }
}
