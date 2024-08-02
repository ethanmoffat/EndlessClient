using System;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Net.Connection;

namespace EOLib.Net.Communication
{
    [AutoMappedType]
    public class SafeNetworkOperationFactory : ISafeNetworkOperationFactory
    {
        private readonly IBackgroundReceiveActions _backgroundReceiveActions;
        private readonly INetworkConnectionActions _networkConnectionActions;

        public SafeNetworkOperationFactory(IBackgroundReceiveActions backgroundReceiveActions,
                                           INetworkConnectionActions networkConnectionActions)
        {
            _backgroundReceiveActions = backgroundReceiveActions;
            _networkConnectionActions = networkConnectionActions;
        }

        public ISafeNetworkOperation CreateSafeBlockingOperation(
            Func<Task> networkOperation,
            Action<NoDataSentException> sendErrorAction = null,
            Action<EmptyPacketReceivedException> receiveErrorAction = null)
        {
            return new SafeBlockingNetworkOperation(
                _backgroundReceiveActions,
                _networkConnectionActions,
                networkOperation,
                sendErrorAction,
                receiveErrorAction);
        }

        public ISafeNetworkOperation<T> CreateSafeBlockingOperation<T>(
            Func<Task<T>> networkOperation,
            Action<NoDataSentException> sendErrorAction = null,
            Action<EmptyPacketReceivedException> receiveErrorAction = null)
        {
            return new SafeBlockingNetworkOperation<T>(
                _backgroundReceiveActions,
                _networkConnectionActions,
                networkOperation,
                sendErrorAction,
                receiveErrorAction);
        }

        public ISafeNetworkOperation CreateSafeAsyncOperation(Func<Task> networkOperation, Action<NoDataSentException> sendErrorAction = null)
        {
            return new SafeAsyncNetworkOperation(
                _backgroundReceiveActions,
                _networkConnectionActions,
                networkOperation,
                sendErrorAction);
        }
    }
}