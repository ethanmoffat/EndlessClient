using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Connection
{
    /// <summary>
    /// Sent when the server is updating the sequence numbers for the client
    /// </summary>
    [AutoMappedType]
    public class ConnectionPlayerHandler : DefaultAsyncPacketHandler<ConnectionPlayerServerPacket>
    {
        private readonly IPacketProcessActions _packetProcessActions;
        private readonly IPacketSendService _packetSendService;

        public override PacketFamily Family => PacketFamily.Connection;

        public override PacketAction Action => PacketAction.Player;

        public override bool CanHandle => true;

        public ConnectionPlayerHandler(IPacketProcessActions packetProcessActions,
                                       IPacketSendService packetSendService)
        {
            _packetProcessActions = packetProcessActions;
            _packetSendService = packetSendService;
        }

        public override bool HandlePacket(ConnectionPlayerServerPacket packet)
        {
            _packetProcessActions.SetSequenceStart(PingSequenceStart.FromPingValues(packet.Seq1, packet.Seq2));

            try
            {
                _packetSendService.SendPacket(new ConnectionPingClientPacket());
            }
            catch (NoDataSentException)
            {
                return false;
            }

            return true;
        }
    }
}
