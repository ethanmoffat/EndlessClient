// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EOLib.Net.Communication;
using EOLib.Net.Connection;

namespace EOLib.Net
{
	public class SafeInBandNetworkOperationFactory : ISafeInBandNetworkOperationFactory
	{
		private readonly IBackgroundReceiveActions _backgroundReceiveActions;
		private readonly INetworkConnectionActions _networkConnectionActions;

		public SafeInBandNetworkOperationFactory(IBackgroundReceiveActions backgroundReceiveActions,
												 INetworkConnectionActions networkConnectionActions)
		{
			_backgroundReceiveActions = backgroundReceiveActions;
			_networkConnectionActions = networkConnectionActions;
		}

		public SafeInBandNetworkOperation<T> CreateSafeOperation<T>(Func<Task<T>> networkOperation, Action errorAction)
		{
			return new SafeInBandNetworkOperation<T>(
				_backgroundReceiveActions,
				_networkConnectionActions,
				networkOperation,
				errorAction);
		}
	}
}
