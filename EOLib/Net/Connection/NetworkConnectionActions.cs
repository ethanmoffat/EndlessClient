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
using EOLib.Net.Translators;

namespace EOLib.Net.Connection
{
	public class NetworkConnectionActions : INetworkConnectionActions
	{
		private readonly INetworkClientRepository _networkClientRepository;
		private readonly IConnectionStateRepository _connectionStateRepository;
		private readonly IConfigurationProvider _configurationProvider;
		private readonly IHashService _hashService;
		private readonly IHDSerialNumberService _hdSerialNumberService;
		private readonly IPacketTranslator<IInitializationData> _initPacketTranslator;
		private readonly INetworkClientFactory _networkClientFactory;
		private readonly IPacketSendService _packetSendService;

		public NetworkConnectionActions(INetworkClientRepository networkClientRepository,
										IConnectionStateRepository connectionStateRepository,
										IConfigurationProvider configurationProvider,
										IHashService hashService,
										IHDSerialNumberService hdSerialNumberService,
										IPacketTranslator<IInitializationData> initPacketTranslator,
										INetworkClientFactory networkClientFactory,
										IPacketSendService packetSendService)
		{
			_networkClientRepository = networkClientRepository;
			_connectionStateRepository = connectionStateRepository;
			_configurationProvider = configurationProvider;
			_hashService = hashService;
			_hdSerialNumberService = hdSerialNumberService;
			_initPacketTranslator = initPacketTranslator;
			_networkClientFactory = networkClientFactory;
			_packetSendService = packetSendService;
		}

		public async Task<ConnectResult> ConnectToServer()
		{
			if (Client.Connected)
				return ConnectResult.AlreadyConnected;

			var host = _configurationProvider.Host;
			var port = _configurationProvider.Port;

			var result = await Client.ConnectToServer(host, port);
			if (result != ConnectResult.AlreadyConnected && result != ConnectResult.Success)
				_connectionStateRepository.NeedsReconnect = true;

			return result;
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

			_connectionStateRepository.NeedsReconnect = true;
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

			var responsePacket = await _packetSendService.SendRawPacketAndWaitAsync(packet);
			if (IsInvalidInitPacket(responsePacket))
				throw new EmptyPacketReceivedException();

			return _initPacketTranslator.TranslatePacket(responsePacket);
		}

		public void CompleteHandshake(IInitializationData initializationData)
		{
			var packet = new PacketBuilder(PacketFamily.Connection, PacketAction.Accept)
				.AddShort((short)initializationData[InitializationDataKey.SendMultiple])
				.AddShort((short)initializationData[InitializationDataKey.ReceiveMultiple])
				.AddShort((short)initializationData[InitializationDataKey.ClientID])
				.Build();

			_packetSendService.SendPacket(packet);
		}

		private static bool IsInvalidInitPacket(IPacket responsePacket)
		{
			return responsePacket.Family != PacketFamily.Init || responsePacket.Action != PacketAction.Init;
		}

		private INetworkClient Client { get { return _networkClientRepository.NetworkClient; } }
	}
}
