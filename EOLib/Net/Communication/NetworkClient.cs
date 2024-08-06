using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EOLib.IO.Services;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Communication
{
    public class NetworkClient : INetworkClient
    {
        private readonly IPacketProcessActions _packetProcessActions;
        private readonly IPacketHandlingActions _packetHandlingActions;
        private readonly INumberEncoderService _numberEncoderService;

        private readonly IAsyncSocket _socket;

        public bool Connected => _socket.Connected;

        public bool Started { get; private set; }

        public TimeSpan ReceiveTimeout { get; }

        public NetworkClient(IPacketProcessActions packetProcessActions,
                             IPacketHandlingActions packetHandlingActions,
                             INumberEncoderService numberEncoderService,
                             TimeSpan receiveTimeout)
        {
            _packetProcessActions = packetProcessActions;
            _packetHandlingActions = packetHandlingActions;
            _numberEncoderService = numberEncoderService;
            ReceiveTimeout = receiveTimeout;

            _socket = new AsyncSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task<ConnectResult> ConnectToServer(string host, int port)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(host, out ip))
            {
                var addressList = Dns.GetHostEntry(host).AddressList;
                var ipv4Addresses = Array.FindAll(addressList, a => a.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4Addresses.Length == 0)
                    return ConnectResult.InvalidEndpoint;

                ip = ipv4Addresses[0];
            }

            var endPoint = new IPEndPoint(ip, port);
            using (var cts = new CancellationTokenSource(5000))
            {
                var task = _socket.ConnectAsync(endPoint, cts.Token);
                await task.ConfigureAwait(false);

                if (task.IsCanceled)
                    return ConnectResult.Timeout;

                Started = task.Result == ConnectResult.Success;

                return task.Result;
            }
        }

        public void Disconnect()
        {
            if (Connected)
                _socket.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);

            Started = false;
        }

        public async Task RunReceiveLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var lengthData = await _socket.ReceiveAsync(2, cancellationToken);
                if (cancellationToken.IsCancellationRequested || lengthData.Length != 2)
                {
                    break;
                }
                var length = _numberEncoderService.DecodeNumber(lengthData);

                var packetData = await _socket.ReceiveAsync(length, cancellationToken);
                if (cancellationToken.IsCancellationRequested || packetData.Length != length)
                {
                    break;
                }

                _packetProcessActions
                    .DecodeData(packetData)
                    .MatchSome(_packetHandlingActions.EnqueuePacketForHandling);
            }
        }

        public int Send(IPacket packet)
        {
            var sendTask = SendAsync(packet);
            return sendTask.Result;
        }

        public async Task<int> SendAsync(IPacket packet, int timeout = 1500)
        {
            var bytesToSend = _packetProcessActions.EncodePacket(packet);
            using var cts = new CancellationTokenSource(timeout);
            return await _socket.SendAsync(bytesToSend, cts.Token).ConfigureAwait(false);
        }

        public async Task<int> SendRawPacketAsync(IPacket packet, int timeout = 1500)
        {
            var bytesToSend = _packetProcessActions.EncodeRawPacket(packet);
            using var cts = new CancellationTokenSource(timeout);
            return await _socket.SendAsync(bytesToSend, cts.Token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NetworkClient()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connected)
                {
                    Disconnect();
                }

                _socket.Dispose();
            }
        }
    }
}
