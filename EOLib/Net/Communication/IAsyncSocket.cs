// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
	public interface IAsyncSocket : IDisposable
	{
		Task<int> SendAsync(byte[] data, CancellationToken ct);

		Task<byte[]> ReceiveAsync(int bytes, CancellationToken ct);

		Task<bool> CheckIsConnectedAsync(CancellationToken ct);

		Task<ConnectResult> ConnectAsync(EndPoint endPoint, CancellationToken ct);

		Task DisconnectAsync(CancellationToken ct);
	}
}
