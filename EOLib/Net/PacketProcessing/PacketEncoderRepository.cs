using AutomaticTypeMapper;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType(IsSingleton = true)]
    public class PacketEncoderRepository : IPacketEncoderRepository
    {
        public byte ReceiveMultiplier { get; set; }
        public byte SendMultiplier { get; set; }
    }

    public interface IPacketEncoderRepository
    {
        byte ReceiveMultiplier { get; set; }
        byte SendMultiplier { get; set; }
    }
}
