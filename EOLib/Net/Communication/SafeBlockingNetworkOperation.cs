using System;
using System.Threading.Tasks;
using EOLib.Net.Connection;

namespace EOLib.Net.Communication
{
    /// <summary>
    /// Wraps an in-band network operation (send/receive), handling send and receive exceptions and executing code when a failure occurs.
    /// <para>By default, disconnects from server and stops receiving on error</para>
    /// </summary>
    public class SafeBlockingNetworkOperation : SafeNetworkOperationBase
    {
        private readonly Func<Task> _operation;

        public SafeBlockingNetworkOperation(IBackgroundReceiveActions backgroundReceiveActions,
                                            INetworkConnectionActions networkConnectionActions,
                                            Func<Task> operation,
                                            Action<NoDataSentException> sendErrorAction = null,
                                            Action<EmptyPacketReceivedException> receiveErrorAction = null)
            : base(backgroundReceiveActions,
                   networkConnectionActions,
                   sendErrorAction,
                   receiveErrorAction)
        {
            _operation = operation;
        }

        protected override async Task DoOperation()
        {
            await _operation();
        }
    }

    public class SafeBlockingNetworkOperation<T> : SafeNetworkOperationBase, ISafeNetworkOperation<T>
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