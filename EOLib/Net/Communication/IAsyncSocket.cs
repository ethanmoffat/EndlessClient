using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public interface IAsyncSocket : IDisposable
    {
        bool Connected { get; }

        Task<int> SendAsync(byte[] data, CancellationToken ct);

        Task<byte[]> ReceiveAsync(int bytes, CancellationToken ct);

        Task<bool> CheckIsConnectedAsync(CancellationToken ct);

        Task<ConnectResult> ConnectAsync(EndPoint endPoint, CancellationToken ct);

        Task DisconnectAsync(CancellationToken ct);
    }
}
