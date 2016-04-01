// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Data;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.Communication
{
	public class NetworkClientFactory : INetworkClientFactory
	{
		private readonly IPacketQueueProvider _packetQueue;
		private readonly IPacketProcessorActions _packetProcessActions;
		private readonly INumberEncoderService _numberEncoderService;

		public NetworkClientFactory(IPacketQueueProvider packetQueue,
									IPacketProcessorActions packetProcessActions,
									INumberEncoderService numberEncoderService)
		{
			_packetQueue = packetQueue;
			_packetProcessActions = packetProcessActions;
			_numberEncoderService = numberEncoderService;
		}

		public INetworkClient<IPacketQueue> CreateNetworkClient()
		{
			return new NetworkClient(_packetQueue, _packetProcessActions, _numberEncoderService);
		}
	}
}
