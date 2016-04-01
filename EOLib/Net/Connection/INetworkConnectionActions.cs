// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Data.Protocol;
using EOLib.Net.Communication;

namespace EOLib.Net.Connection
{
	public interface INetworkConnectionActions
	{
		Task<ConnectResult> ConnectToServer();

		Task<ConnectResult> ReconnectToServer();

		void DisconnectFromServer();

		Task<IInitializationData> BeginHandshake();

		void CompleteHandshake();
	}
}
