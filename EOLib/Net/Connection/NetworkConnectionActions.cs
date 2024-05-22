using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Domain.Login;
using EOLib.Net.Communication;
using EOLib.Net.PacketProcessing;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;
using System.Threading.Tasks;
using Version = Moffat.EndlessOnline.SDK.Protocol.Net.Version;

namespace EOLib.Net.Connection
{
    [AutoMappedType]
    public class NetworkConnectionActions : INetworkConnectionActions
    {
        private readonly INetworkClientRepository _networkClientRepository;
        private readonly ISequenceRepository _sequenceRepository;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IHDSerialNumberService _hdSerialNumberService;
        private readonly INetworkClientFactory _networkClientFactory;
        private readonly IPacketSendService _packetSendService;
        private readonly IPlayerInfoRepository _playerInfoRepository;


        public NetworkConnectionActions(INetworkClientRepository networkClientRepository,
                                        ISequenceRepository sequenceRepository,
                                        IConfigurationProvider configurationProvider,
                                        IHDSerialNumberService hdSerialNumberService,
                                        INetworkClientFactory networkClientFactory,
                                        IPacketSendService packetSendService,
                                        IPlayerInfoRepository playerInfoRepository)
        {
            _networkClientRepository = networkClientRepository;
            _sequenceRepository = sequenceRepository;
            _configurationProvider = configurationProvider;
            _hdSerialNumberService = hdSerialNumberService;
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

            _sequenceRepository.ResetState();
        }

        public async Task<InitInitServerPacket> BeginHandshake(int challenge)
        {
            var hdSerialNumber = _hdSerialNumberService.GetHDSerialNumber();

            var initPacket = new InitInitClientPacket
            {
                Challenge = challenge,
                Version = new Version
                {
                    Major = _configurationProvider.VersionMajor,
                    Minor = _configurationProvider.VersionMinor,
                    Patch = _configurationProvider.VersionBuild,
                },
                Hdid = hdSerialNumber
            };

            var responsePacket = await _packetSendService.SendRawPacketAndWaitAsync(initPacket);
            if (IsInvalidInitPacket(responsePacket, out var initInitServerPacket))
                throw new EmptyPacketReceivedException();

            return initInitServerPacket;
        }

        public void CompleteHandshake(InitInitServerPacket serverPacket)
        {
            if (serverPacket.ReplyCode != InitReply.Ok || !(serverPacket.ReplyCodeData is InitInitServerPacket.ReplyCodeDataOk okData))
                throw new InvalidOperationException($"Unable to complete handshake for response code: {serverPacket.ReplyCode}");

            _playerInfoRepository.PlayerID = okData.PlayerId;

            var packet = new ConnectionAcceptClientPacket
            {
                ClientEncryptionMultiple = okData.ClientEncryptionMultiple,
                ServerEncryptionMultiple = okData.ServerEncryptionMultiple,
                PlayerId = okData.PlayerId,
            };

            _packetSendService.SendPacket(packet);
        }

        private static bool IsInvalidInitPacket(IPacket responsePacket, out InitInitServerPacket serverPacket)
        {
            var idMatches = responsePacket.Family != PacketFamily.Init || responsePacket.Action != PacketAction.Init;
            serverPacket = responsePacket as InitInitServerPacket;
            return idMatches && serverPacket != null;
        }

        private INetworkClient Client => _networkClientRepository.NetworkClient;
    }
}
