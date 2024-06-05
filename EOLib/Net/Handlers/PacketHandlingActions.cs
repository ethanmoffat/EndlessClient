using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Handlers
{
    [AutoMappedType]
    public class PacketHandlingActions : IPacketHandlingActions
    {
        private readonly IPacketQueueProvider _packetQueueProvider;
        private readonly IPacketHandlingTypeFinder _packetHandlingTypeFinder;
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public PacketHandlingActions(IPacketQueueProvider packetQueueProvider,
                                     IPacketHandlingTypeFinder packetHandlingTypeFinder,
                                     IPlayerInfoProvider playerInfoProvider)
        {
            _packetQueueProvider = packetQueueProvider;
            _packetHandlingTypeFinder = packetHandlingTypeFinder;
            _playerInfoProvider = playerInfoProvider;
        }

        public void EnqueuePacketForHandling(IPacket packet)
        {
            if (_playerInfoProvider.PlayerIsInGame)
            {
                // all in-game packets should be handled out-of-band
                _packetQueueProvider.HandleOutOfBandPacketQueue.EnqueuePacketForHandling(packet);
            }
            else
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
    }

    public interface IPacketHandlingActions
    {
        void EnqueuePacketForHandling(IPacket packet);
    }
}