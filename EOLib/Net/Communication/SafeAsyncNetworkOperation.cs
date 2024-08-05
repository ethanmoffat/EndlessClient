using System;
using System.Threading.Tasks;
using EOLib.Net.Connection;

namespace EOLib.Net.Communication
{
    public class SafeAsyncNetworkOperation : SafeNetworkOperationBase
    {
        private readonly Func<Task> _networkOperation;

        public SafeAsyncNetworkOperation(IBackgroundReceiveActions backgroundReceiveActions,
                                         INetworkConnectionActions networkConnectionActions,
                                         Func<Task> networkOperation,
                                         Action<NoDataSentException> sendErrorAction)
            : base(backgroundReceiveActions,
            networkConnectionActions,
            sendErrorAction,
            delegate { })
        {
            _networkOperation = networkOperation;
        }

        protected override async Task DoOperation()
        {
            await _networkOperation();
        }
    }
}