using System;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using EOLib.Net.Communication;
using EOLib.Net.PacketProcessing;
using EOLib.Net.Translators;

namespace EOLib.Net.Connection
{
    [AutoMappedType]
    public class NetworkConnectionActions : INetworkConnectionActions
    {
        private readonly INetworkClientRepository _networkClientRepository;
        private readonly ISequenceRepository _sequenceRepository;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IHashService _hashService;
        private readonly IHDSerialNumberService _hdSerialNumberService;
        private readonly IPacketTranslator<IInitializationData> _initPacketTranslator;
        private readonly INetworkClientFactory _networkClientFactory;
        private readonly IPacketSendService _packetSendService;
        private readonly IPlayerInfoRepository _playerInfoRepository;


        public NetworkConnectionActions(INetworkClientRepository networkClientRepository,
                                        ISequenceRepository sequenceRepository,
                                        IConfigurationProvider configurationProvider,
                                        IHashService hashService,
                                        IHDSerialNumberService hdSerialNumberService,
                                        IPacketTranslator<IInitializationData> initPacketTranslator,
                                        INetworkClientFactory networkClientFactory,
                                        IPacketSendService packetSendService,
                                        IPlayerInfoRepository playerInfoRepository)
        {
            _networkClientRepository = networkClientRepository;
            _sequenceRepository = sequenceRepository;
            _configurationProvider = configurationProvider;
            _hashService = hashService;
            _hdSerialNumberService = hdSerialNumberService;
            _initPacketTranslator = initPacketTranslator;
            _networkClientFactory = networkClientFactory;
            _packetSendService = packetSendService;
            _playerInfoRepository = playerInfoRepository;
        }

        public async Task<ConnectResult> ConnectToServer()
        {
            if (Client != null && Client.Connected)
                return ConnectResult.AlreadyConnected;

            _networkClientRepository.NetworkClient?.Dispose();
            _networkClientRepository.NetworkClient = _networkClientFactory.CreateNetworkClient();

            var host = _configurationProvider.Host;
            var port = _configurationProvider.Port;

            return await Client.ConnectToServer(host, port);
        }

        public void DisconnectFromServer()
        {
            if (Client != null)
            {
                Client.Disconnect();
                Client.Dispose();
                _networkClientRepository.NetworkClient = null;
            }

            _sequenceRepository.SequenceIncrement = 0;
            _sequenceRepository.SequenceStart = 0;
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
            _playerInfoRepository.PlayerID = (short)initializationData[InitializationDataKey.PlayerID];

            var packet = new PacketBuilder(PacketFamily.Connection, PacketAction.Accept)
                .AddShort((short)initializationData[InitializationDataKey.SendMultiple])
                .AddShort((short)initializationData[InitializationDataKey.ReceiveMultiple])
                .AddShort(_playerInfoRepository.PlayerID)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        private static bool IsInvalidInitPacket(IPacket responsePacket)
        {
            return responsePacket.Family != PacketFamily.Init || responsePacket.Action != PacketAction.Init;
        }

        private INetworkClient Client => _networkClientRepository.NetworkClient;
    }
}
