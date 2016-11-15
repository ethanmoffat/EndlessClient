// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Logger;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;

namespace EOLib.PacketHandlers
{
    /// <summary>
    /// Handles incoming CONNECTION_PLAYER packets which are used for updating sequence numbers in the EO protocol
    /// </summary>
    public class ConnectionPlayerHandler : DefaultAsyncPacketHandler
    {
        private readonly IPacketProcessActions _packetProcessActions;
        private readonly IPacketSendService _packetSendService;
        private readonly ILoggerProvider _loggerProvider;

        public override PacketFamily Family { get { return PacketFamily.Connection; } }

        public override PacketAction Action { get { return PacketAction.Player; } }

        public override bool CanHandle { get { return true; } }

        public ConnectionPlayerHandler(IPacketProcessActions packetProcessActions,
                                       IPacketSendService packetSendService,
                                       ILoggerProvider loggerProvider)
        {
            _packetProcessActions = packetProcessActions;
            _packetSendService = packetSendService;
            _loggerProvider = loggerProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var seq1 = packet.ReadShort();
            var seq2 = packet.ReadChar();

            _packetProcessActions.SetUpdatedBaseSequenceNumber(seq1, seq2);

            var response = new PacketBuilder(PacketFamily.Connection, PacketAction.Ping).Build();
            try
            {
                _packetSendService.SendPacketAsync(response)
                                  .Wait();
            }
            catch (NoDataSentException)
            {
                return false;
            }

            return true;
        }
    }
}
