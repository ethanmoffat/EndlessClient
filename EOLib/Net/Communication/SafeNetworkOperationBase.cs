using EOLib.Net.Connection;
using System;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public abstract class SafeNetworkOperationBase : ISafeNetworkOperation
    {
        private readonly IBackgroundReceiveActions _backgroundReceiveActions;
        private readonly INetworkConnectionActions _networkConnectionActions;
        private readonly Action<NoDataSentException> _sendErrorAction;
        private readonly Action<EmptyPacketReceivedException> _receiveErrorAction;

        protected SafeNetworkOperationBase(IBackgroundReceiveActions backgroundReceiveActions,
                                           INetworkConnectionActions networkConnectionActions,
                                           Action<NoDataSentException> sendErrorAction = null,
                                           Action<EmptyPacketReceivedException> receiveErrorAction = null)
        {
            _backgroundReceiveActions = backgroundReceiveActions;
            _networkConnectionActions = networkConnectionActions;
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

        protected abstract Task DoOperation();

        private void DisconnectAndStopReceiving()
        {
            _backgroundReceiveActions.CancelBackgroundReceiveLoop();
            _networkConnectionActions.DisconnectFromServer();
        }
    }
}