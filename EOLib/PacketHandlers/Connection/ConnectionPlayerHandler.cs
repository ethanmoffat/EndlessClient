using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;

namespace EOLib.PacketHandlers.Connection
{
    /// <summary>
    /// Sent when the server is updating the sequence numbers for the client
    /// </summary>
    [AutoMappedType]
    public class ConnectionPlayerHandler : DefaultAsyncPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var seq1 = packet.ReadShort();
            var seq2 = packet.ReadChar();

            _packetProcessActions.SetUpdatedBaseSequenceNumber(seq1, seq2);

            var response = new PacketBuilder(PacketFamily.Connection, PacketAction.Ping)
                .AddString("k")
                .Build();

            try
            {
                _packetSendService.SendPacket(response);
            }
            catch (NoDataSentException)
            {
                return false;
            }

            return true;
        }
    }
}
