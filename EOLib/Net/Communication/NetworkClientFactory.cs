// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Services;
using EOLib.Logger;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.Communication
{
    public class NetworkClientFactory : INetworkClientFactory
    {
        private readonly IPacketProcessActions _packetProcessActions;
        private readonly IPacketHandlingActions _packetHandlingActions;
        private readonly INumberEncoderService _numberEncoderService;
        private readonly ILoggerProvider _loggerProvider;

        public NetworkClientFactory(IPacketProcessActions packetProcessActions,
                                    IPacketHandlingActions packetHandlingActions,
                                    INumberEncoderService numberEncoderService,
                                    ILoggerProvider loggerProvider)
        {
            _packetProcessActions = packetProcessActions;
            _packetHandlingActions = packetHandlingActions;
            _numberEncoderService = numberEncoderService;
            _loggerProvider = loggerProvider;
        }

        public INetworkClient CreateNetworkClient()
        {
            return new NetworkClient(_packetProcessActions, _packetHandlingActions, _numberEncoderService, _loggerProvider);
        }
    }
}
