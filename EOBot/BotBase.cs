// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading;
using EOLib.Config;
using EOLib.IO.Services;
using EOLib.Logger;
using EOLib.Net;
using EOLib.Net.API;
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

        //derived classes can modify this part
        protected readonly int _index;
        private readonly string _host;
        private readonly int _port;
        private EOClient _client;
        protected PacketAPI _api;

        //unneeded for now
        ///// <summary>
        ///// Get whether or not the worker thread was requested to terminate via a call to Terminate()
        ///// </summary>
        //public bool TerminationRequested { get { return _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested; } }

        /// <summary>
        /// Invoked once work has completed executing.
        /// </summary>
        public event Action WorkCompleted;

        protected BotBase(int botIndex, string host, int port)
        {
            _index = botIndex;
            _host = host;
            _port = port;

            _terminationEvent = new AutoResetEvent(false);
            _cancelTokenSource = new CancellationTokenSource();
        }

        //all bots are going to want to do the init handshake with the server
        public virtual void Initialize()
        {
            _client = new EOClient(CreatePacketProcessorActions());
            if (!_client.ConnectToServer(_host, _port))
                throw new ArgumentException($"Bot {_index}: Unable to connect to server! Host={_host} Port={_port}");
            _api = new PacketAPI(_client);

            //todo: adapt to new networking architecture
            //InitData data;
            //if (!_api.Initialize(0, 0, 28, new HDSerialNumberService().GetHDSerialNumber(), out data))
            //    throw new TimeoutException(string.Format("Bot {0}: Failed initialization handshake with server!", _index));
            //_client.SetInitData(data);

            //if (!_api.ConfirmInit(data.emulti_e, data.emulti_d, data.clientID))
            //    throw new TimeoutException(string.Format("Bot {0}: Failed initialization handshake with server!", _index));

            //if (!_api.Initialized || !_client.ConnectedAndInitialized || data.ServerResponse != InitReply.INIT_OK)
            //    throw new InvalidOperationException(string.Format("Bot {0}: Invalid response from server or connection failed! Must receive an OK reply.", _index));

            //_initialized = true;
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
        protected abstract void DoWork(CancellationToken ct);

        private void DoWorkOnly()
        {
            DoWork(_cancelTokenSource.Token);
            FireWorkCompleted();
        }

        private void DoWorkAndWaitForTermination()
        {
            DoWork(_cancelTokenSource.Token);
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

                if (_workerThread != null)
                    _workerThread.Join();

                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }

                if (_api != null)
                {
                    _api.Dispose();
                    _api = null;
                }

                if (_terminationEvent != null)
                {
                    _terminationEvent.Dispose();
                    _terminationEvent = null;
                }

                if (_cancelTokenSource != null)
                {
                    _cancelTokenSource.Dispose();
                    _cancelTokenSource = null;
                }
            }
        }

        private static PacketProcessActions CreatePacketProcessorActions()
        {
            return new PacketProcessActions(new SequenceRepository(),
                                            new PacketEncoderRepository(),
                                            new PacketEncoderService(new NumberEncoderService()),
                                            new PacketSequenceService(),
                                            new LoggerProvider(new LoggerFactory(new ConfigurationRepository())));
        }
    }
}
