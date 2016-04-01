// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
	public interface INetworkClient<out T> : IDisposable
		where T : IPacketQueue
	{
		//todo: make this private and create PacketQueueRepository
		T PacketQueue { get; }

		bool Connected { get; }

		Task<ConnectResult> ConnectToServer(string host, int port);

		void Disconnect();

		Task RunReceiveLoopAsync();

		void CancelBackgroundReceiveLoop();

		int Send(IPacket packet);

		Task<int> SendAsync(IPacket packet, int timeout);
	}
}
