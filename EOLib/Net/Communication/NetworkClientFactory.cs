// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.Communication
{
    public class NetworkClientFactory : INetworkClientFactory
    {
        private readonly IPacketProcessorActions _packetProcessActions;
        private readonly IPacketHandlingActions _packetHandlingActions;
        private readonly INumberEncoderService _numberEncoderService;

        public NetworkClientFactory(IPacketProcessorActions packetProcessActions,
                                    IPacketHandlingActions packetHandlingActions,
                                    INumberEncoderService numberEncoderService)
        {
            _packetProcessActions = packetProcessActions;
            _packetHandlingActions = packetHandlingActions;
            _numberEncoderService = numberEncoderService;
        }

        public INetworkClient CreateNetworkClient()
        {
            return new NetworkClient(_packetProcessActions, _packetHandlingActions, _numberEncoderService);
        }
    }
}
