using AutomaticTypeMapper;
using EOLib.IO.Services;
using EOLib.Logger;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;
using System;

namespace EOLib.Net.Communication
{
    [AutoMappedType]
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

        public INetworkClient CreateNetworkClient(int timeout = Constants.ResponseTimeout)
        {
            return new NetworkClient(_packetProcessActions, _packetHandlingActions, _numberEncoderService, _loggerProvider, TimeSpan.FromMilliseconds(timeout));
        }
    }
}
