// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
	public interface INetworkClient<T> : IDisposable
		where T : IPacketQueue
	{
		T PacketQueue { get; }

		bool Connected { get; }

		Task<ConnectResult> ConnectToServer(string host, int port);

		void Disconnect();

		void StartBackgroundReceiveLoop();

		void CancelBackgroundReceiveLoop();

		int Send(IPacket packet);

		Task<int> SendAsync(IPacket packet, int timeout);
	}
}
