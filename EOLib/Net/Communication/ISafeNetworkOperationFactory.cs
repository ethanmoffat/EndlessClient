// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
    }
}