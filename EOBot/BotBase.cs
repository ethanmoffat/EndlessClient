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
    internal abstract class BotBase : IBot
    {
        //base class implementation - should not be modified in derived classes
        private Thread _workerThread;
        private AutoResetEvent _terminationEvent;
        private CancellationTokenSource _cancelTokenSource;
        private bool _initialized;

        protected readonly int _index;
        protected bool TerminationRequested => _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested;

        public event Action WorkCompleted;

        protected BotBase(int botIndex)
        {
            _index = botIndex;

            _terminationEvent = new AutoResetEvent(false);
            _cancelTokenSource = new CancellationTokenSource();
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

        /// <summary>
        /// Run the bot
        /// </summary>
        /// <param name="waitForTermination">True to keep running until Terminate() is called, false otherwise</param>
        public void Run(bool waitForTermination)
        {
            if (!_initialized)
                throw new InvalidOperationException("Must call Initialize() before calling Run()");

            var doWorkAndWait = new ThreadStart(DoWorkAndWaitForTermination);
            var doWork = new ThreadStart(DoWorkOnly);

            _workerThread = new Thread(waitForTermination ? doWorkAndWait : doWork);
            _workerThread.Start();
        }

        /// <summary>
        /// Abstract worker method. Override with custom work logic for the bot to execute
        /// </summary>
        /// <param name="ct">A cancellation token that will be signalled when Terminate() is called</param>
        protected abstract Task DoWorkAsync(CancellationToken ct);

        private async void DoWorkOnly()
        {
            await DoWorkAsync(_cancelTokenSource.Token);
            FireWorkCompleted();
        }

        private async void DoWorkAndWaitForTermination()
        {
            await DoWorkAsync(_cancelTokenSource.Token);
            _terminationEvent.WaitOne();
            FireWorkCompleted();
        }

        /// <summary>
        /// Terminate the bot. Ends execution as soon as is convenient.
        /// </summary>
        public void Terminate()
        {
            _cancelTokenSource.Cancel();
            _terminationEvent.Set();
        }

        private void FireWorkCompleted()
        {
            if (WorkCompleted != null)
                WorkCompleted();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~BotBase()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Terminate();

                _workerThread?.Join();

                _terminationEvent?.Dispose();
                _terminationEvent = null;

                _cancelTokenSource?.Dispose();
                _cancelTokenSource = null;
            }
        }
    }
}
