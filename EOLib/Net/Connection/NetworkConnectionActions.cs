// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EOLib.Data.Protocol;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using EOLib.Net.Communication;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.Connection
{
	public class NetworkConnectionActions : INetworkConnectionActions
	{
		private readonly INetworkClientRepository _networkClientRepository;
		private readonly IConfigurationProvider _configurationProvider;
		private readonly IPacketQueueProvider _packetQueueProvider;
		private readonly IHashService _hashService;
		private readonly IHDSerialNumberService _hdSerialNumberService;
		private readonly IInitDataGeneratorService _initDataGeneratorService;
		private readonly INetworkClientFactory _networkClientFactory;

		public NetworkConnectionActions(INetworkClientRepository networkClientRepository,
										IConfigurationProvider configurationProvider,
										IPacketQueueProvider packetQueueProvider,
										IHashService hashService,
										IHDSerialNumberService hdSerialNumberService,
										IInitDataGeneratorService initDataGeneratorService,
										INetworkClientFactory networkClientFactory)
		{
			_networkClientRepository = networkClientRepository;
			_configurationProvider = configurationProvider;
			_packetQueueProvider = packetQueueProvider;
			_hashService = hashService;
			_hdSerialNumberService = hdSerialNumberService;
			_initDataGeneratorService = initDataGeneratorService;
			_networkClientFactory = networkClientFactory;
		}

		public async Task<ConnectResult> ConnectToServer()
		{
			if (Client.Connected)
				return ConnectResult.AlreadyConnected;

			var host = _configurationProvider.Host;
			var port = _configurationProvider.Port;

			return await Client.ConnectToServer(host, port);
		}

		public async Task<ConnectResult> ReconnectToServer()
		{
			if (Client.Connected)
			{
				Client.CancelBackgroundReceiveLoop();
				Client.Disconnect();
			}
			Client.Dispose();

			_networkClientRepository.NetworkClient = _networkClientFactory.CreateNetworkClient();
			return await ConnectToServer();
		}

		public void DisconnectFromServer()
		{
			if (Client.Connected)
				Client.Disconnect();
		}

		public async Task<IInitializationData> BeginHandshake()
		{
			var stupidHash = _hashService.StupidHash(new Random().Next(6, 12));
			var hdSerialNumber = _hdSerialNumberService.GetHDSerialNumber();

			var packet = new PacketBuilder(PacketFamily.Init, PacketAction.Init)
				.AddThree(stupidHash)
				.AddChar(_configurationProvider.VersionMajor)
				.AddChar(_configurationProvider.VersionMinor)
				.AddChar(_configurationProvider.VersionBuild)
				.AddChar(112)
				.AddChar((byte)hdSerialNumber.Length)
				.AddString(hdSerialNumber)
				.Build();

			var bytes = await Client.SendRawPacketAsync(packet);
			if (bytes == 0)
				throw new NoDataSentException();

			var responsePacket = await _packetQueueProvider.PacketQueue.WaitForPacketAndDequeue();
			if (IsInvalidInitPacket(responsePacket))
				throw new EmptyPacketReceivedException();

			return _initDataGeneratorService.GetInitData(responsePacket);
		}

		public void CompleteHandshake(IInitializationData initializationData)
		{
			var packet = new PacketBuilder(PacketFamily.Connection, PacketAction.Accept)
				.AddShort((short)initializationData[InitializationDataKey.SendMultiple])
				.AddShort((short)initializationData[InitializationDataKey.ReceiveMultiple])
				.AddShort((short)initializationData[InitializationDataKey.ClientID])
				.Build();

			var bytes = Client.Send(packet);
			if (bytes == 0)
				throw new NoDataSentException();
		}

		private static bool IsInvalidInitPacket(IPacket responsePacket)
		{
			return responsePacket is EmptyPacket || (responsePacket.Family != PacketFamily.Init && responsePacket.Action != PacketAction.Init);
		}

		private INetworkClient Client { get { return _networkClientRepository.NetworkClient; } }
	}
}
