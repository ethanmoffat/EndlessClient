using AutomaticTypeMapper;

namespace EOLib.Net.Communication
{
    public interface IPacketQueueRepository
    {
        IPacketQueue IncomingPacketQueue { get; set; }

        IWaitablePacketQueue HandleInBandPacketQueue { get; set; }

        IPacketQueue HandleOutOfBandPacketQueue { get; set; }
    }

    public interface IPacketQueueProvider
    {
        IPacketQueue IncomingPacketQueue { get; }

        IWaitablePacketQueue HandleInBandPacketQueue { get; }

        IPacketQueue HandleOutOfBandPacketQueue { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PacketQueueRepository : IPacketQueueRepository, IPacketQueueProvider
    {
        public IPacketQueue IncomingPacketQueue { get; set; }

        public IWaitablePacketQueue HandleInBandPacketQueue { get; set; }

        public IPacketQueue HandleOutOfBandPacketQueue { get; set; }

        public PacketQueueRepository()
        {
            IncomingPacketQueue = new PacketQueue();
            HandleInBandPacketQueue = new PacketQueue();
            HandleOutOfBandPacketQueue = new PacketQueue();
        }
    }
}