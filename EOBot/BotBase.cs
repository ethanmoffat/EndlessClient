using System;
using System.Threading;
using System.Threading.Tasks;
using EOLib.Config;
using EOLib.Domain.Protocol;
using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;

namespace EOBot
{
    public abstract class BotBase : IBot
    {
        protected bool _initialized;

        protected readonly int _index;

        public event Action WorkCompleted;

        protected BotBase(int botIndex)
        {
            _index = botIndex;
        }

        //all bots are going to want to do the init handshake with the server
        public virtual async Task InitializeAsync(string host, int port)
        {
            var c = DependencyMaster.TypeRegistry[_index];

            var networkClientRepository = c.Resolve<INetworkClientRepository>();
            var networkClientFactory = c.Resolve<INetworkClientFactory>();
            networkClientRepository.NetworkClient = networkClientFactory.CreateNetworkClient();

            var configRepo = c.Resolve<IConfigurationRepository>();
            configRepo.Host = host;
            configRepo.Port = port;

            configRepo.VersionMajor = 0;
            configRepo.VersionMinor = 0;
            configRepo.VersionBuild = 29;

            var connectionActions = c.Resolve<INetworkConnectionActions>();
            var connectResult = await connectionActions.ConnectToServer();
            if (connectResult != ConnectResult.Success)
                throw new ArgumentException($"Bot {_index}: Unable to connect to server! Host={host} Port={port}");

            var backgroundReceiveActions = c.Resolve<IBackgroundReceiveActions>();
            backgroundReceiveActions.RunBackgroundReceiveLoop();
            WorkCompleted += () =>
            {
                backgroundReceiveActions.CancelBackgroundReceiveLoop();
                connectionActions.DisconnectFromServer();
            };

            var handshakeResult = await connectionActions.BeginHandshake();

            if (handshakeResult.Response != InitReply.Success)
                throw new InvalidOperationException(string.Format("Bot {0}: Invalid response from server or connection failed! Must receive an OK reply.", _index));

            var packetProcessActions = c.Resolve<IPacketProcessActions>();

            packetProcessActions.SetInitialSequenceNumber(handshakeResult[InitializationDataKey.SequenceByte1],
                handshakeResult[InitializationDataKey.SequenceByte2]);
            packetProcessActions.SetEncodeMultiples((byte)handshakeResult[InitializationDataKey.ReceiveMultiple],
                (byte)handshakeResult[InitializationDataKey.SendMultiple]);

            connectionActions.CompleteHandshake(handshakeResult);

            _initialized = true;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            if (!_initialized)
                throw new InvalidOperationException("Initialize must be called before calling RunAsync");

            await DoWorkAsync(ct);
            WorkCompleted?.Invoke();
        }

        /// <summary>
        /// Abstract worker method. Override with custom work logic for the bot to execute
        /// </summary>
        /// <param name="ct">A cancellation token that will be signalled when Terminate() is called</param>
        protected abstract Task DoWorkAsync(CancellationToken ct);
    }
}
