using AutomaticTypeMapper;
using EOLib.Net.Communication;

namespace EOLib.Net.Handlers
{
    [AutoMappedType]
    public class PacketHandlingActions : IPacketHandlingActions
    {
        private readonly IPacketQueueProvider _packetQueueProvider;
        private readonly IPacketHandlingTypeFinder _packetHandlingTypeFinder;

        public PacketHandlingActions(IPacketQueueProvider packetQueueProvider,
                                     IPacketHandlingTypeFinder packetHandlingTypeFinder)
        {
            _packetQueueProvider = packetQueueProvider;
            _packetHandlingTypeFinder = packetHandlingTypeFinder;
        }

        public void EnqueuePacketForHandling(IPacket packet)
        {
            var handleType = _packetHandlingTypeFinder.FindHandlingType(packet.Family, packet.Action);

            switch (handleType)
            {
                case PacketHandlingType.InBand:
                    _packetQueueProvider.HandleInBandPacketQueue.EnqueuePacketAndSignalConsumer(packet);
                    break;
                case PacketHandlingType.OutOfBand:
                    _packetQueueProvider.HandleOutOfBandPacketQueue.EnqueuePacketForHandling(packet);
                    break;
                /*default: don't handle the received packet*/
            }
        }
    }

    public interface IPacketHandlingActions
    {
        void EnqueuePacketForHandling(IPacket packet);
    }
}