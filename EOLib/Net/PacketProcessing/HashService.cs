using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Data;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType]
    public class HashService : IHashService
    {
        public int StupidHash(int challenge) => ServerVerifier.Hash(challenge);
    }

    public interface IHashService
    {
        int StupidHash(int challenge);
    }
}
