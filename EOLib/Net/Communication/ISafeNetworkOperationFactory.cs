using System;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public interface ISafeNetworkOperationFactory
    {
        ISafeNetworkOperation CreateSafeBlockingOperation(
            Func<Task> networkOperation,
            Action<NoDataSentException> sendErrorAction = null,
            Action<EmptyPacketReceivedException> receiveErrorAction = null);

        ISafeNetworkOperation<T> CreateSafeBlockingOperation<T>(
            Func<Task<T>> networkOperation,
            Action<NoDataSentException> sendErrorAction = null,
            Action<EmptyPacketReceivedException> receiveErrorAction = null);

        ISafeNetworkOperation CreateSafeAsyncOperation(
            Func<Task> networkOperation,
            Action<NoDataSentException> sendErrorAction = null);
    }
}