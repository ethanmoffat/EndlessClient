// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EOLib.Net.Connection;

namespace EOLib.Net.Communication
{
    /// <summary>
    /// Wraps an in-band network operation (send/receive), handling send and receive exceptions and executing code when a failure occurs.
    /// <para>By default, disconnects from server and stops receiving on error</para>
    /// </summary>
    public class SafeBlockingNetworkOperation : ISafeNetworkOperation
    {
        private readonly IBackgroundReceiveActions _backgroundReceiveActions;
        private readonly INetworkConnectionActions _networkConnectionActions;
        private readonly Func<Task> _operation;
        private readonly Action<NoDataSentException> _sendErrorAction;
        private readonly Action<EmptyPacketReceivedException> _receiveErrorAction;

        public SafeBlockingNetworkOperation(IBackgroundReceiveActions backgroundReceiveActions,
                                            INetworkConnectionActions networkConnectionActions,
                                            Func<Task> operation,
                                            Action<NoDataSentException> sendErrorAction = null,
                                            Action<EmptyPacketReceivedException> receiveErrorAction = null)
        {
            _backgroundReceiveActions = backgroundReceiveActions;
            _networkConnectionActions = networkConnectionActions;
            _operation = operation;
            _sendErrorAction = sendErrorAction ?? (_ => { });
            _receiveErrorAction = receiveErrorAction ?? (_ => { });
        }

        public async Task<bool> Invoke()
        {
            try
            {
                await DoOperation();
                return true;
            }
            catch (NoDataSentException ex) { _sendErrorAction(ex); }
            catch (EmptyPacketReceivedException ex) { _receiveErrorAction(ex); }

            DisconnectAndStopReceiving();
            return false;
        }

        protected virtual async Task DoOperation()
        {
            await _operation();
        }

        private void DisconnectAndStopReceiving()
        {
            _backgroundReceiveActions.CancelBackgroundReceiveLoop();
            _networkConnectionActions.DisconnectFromServer();
        }
    }

    public class SafeBlockingNetworkOperation<T> : SafeBlockingNetworkOperation, ISafeNetworkOperation<T>
    {
        private readonly Func<Task<T>> _operation;

        public T Result { get; private set; }

        public SafeBlockingNetworkOperation(IBackgroundReceiveActions backgroundReceiveActions,
                                            INetworkConnectionActions networkConnectionActions,
                                            Func<Task<T>> operation,
                                            Action<NoDataSentException> sendErrorAction = null,
                                            Action<EmptyPacketReceivedException> receiveErrorAction = null)
            : base(backgroundReceiveActions,
                   networkConnectionActions,
                   operation,
                   sendErrorAction,
                   receiveErrorAction)
        {
            _operation = operation;
        }

        protected override async Task DoOperation()
        {
            Result = await _operation();
        }
    }
}
