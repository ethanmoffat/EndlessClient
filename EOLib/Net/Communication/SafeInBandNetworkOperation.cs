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
	/// <typeparam name="T">The expected output data of the network operation</typeparam>
	public class SafeInBandNetworkOperation<T>
	{
		private readonly IBackgroundReceiveActions _backgroundReceiveActions;
		private readonly INetworkConnectionActions _networkConnectionActions;
		private readonly Func<Task<T>> _operation;
		private readonly Action<NoDataSentException> _sendErrorAction;
		private readonly Action<EmptyPacketReceivedException> _receiveErrorAction;

		public T Result { get; private set; }

		public SafeInBandNetworkOperation(IBackgroundReceiveActions backgroundReceiveActions,
										  INetworkConnectionActions networkConnectionActions,
										  Func<Task<T>> operation,
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
				Result = await _operation();
				return true;
			}
			catch (NoDataSentException ex) { _sendErrorAction(ex); }
			catch (EmptyPacketReceivedException ex) { _receiveErrorAction(ex); }

			DisconnectAndStopReceiving();
			return false;
		}

		private void DisconnectAndStopReceiving()
		{
			_backgroundReceiveActions.CancelBackgroundReceiveLoop();
			_networkConnectionActions.DisconnectFromServer();
		}
	}

	public class SafeInBandNetworkOperation
	{
		private readonly IBackgroundReceiveActions _backgroundReceiveActions;
		private readonly INetworkConnectionActions _networkConnectionActions;
		private readonly Func<Task> _operation;
		private readonly Action<NoDataSentException> _sendErrorAction;
		private readonly Action<EmptyPacketReceivedException> _receiveErrorAction;

		public SafeInBandNetworkOperation(IBackgroundReceiveActions backgroundReceiveActions,
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
				await _operation();
				return true;
			}
			catch (NoDataSentException ex) { _sendErrorAction(ex); }
			catch (EmptyPacketReceivedException ex) { _receiveErrorAction(ex); }

			DisconnectAndStopReceiving();
			return false;
		}

		private void DisconnectAndStopReceiving()
		{
			_backgroundReceiveActions.CancelBackgroundReceiveLoop();
			_networkConnectionActions.DisconnectFromServer();
		}
	}
}
