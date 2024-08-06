using AutomaticTypeMapper;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType(IsSingleton = true)]
    public class PacketEncoderRepository : IPacketEncoderRepository
    {
        public int ReceiveMultiplier { get; set; }
        public int SendMultiplier { get; set; }
    }

    public interface IPacketEncoderRepository
    {
        int ReceiveMultiplier { get; set; }
        int SendMultiplier { get; set; }
    }
}
