using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EOLib.IO.Services;
using EOLib.Logger;
using EOLib.Net.Handlers;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.Communication
{
    public class NetworkClient : INetworkClient
    {
        private readonly IPacketProcessActions _packetProcessActions;
        private readonly IPacketHandlingActions _packetHandlingActions;
        private readonly INumberEncoderService _numberEncoderService;
        private readonly ILoggerProvider _loggerProvider;

        private readonly IAsyncSocket _socket;

        private readonly CancellationTokenSource _backgroundReceiveCTS;
        
        public bool Connected => _socket.Connected;

        public bool Started { get; private set; }

        public NetworkClient(IPacketProcessActions packetProcessActions,
                             IPacketHandlingActions packetHandlingActions,
                             INumberEncoderService numberEncoderService,
                             ILoggerProvider loggerProvider)
        {
            _packetProcessActions = packetProcessActions;
            _packetHandlingActions = packetHandlingActions;
            _numberEncoderService = numberEncoderService;
            _loggerProvider = loggerProvider;

            _socket = new AsyncSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            _backgroundReceiveCTS = new CancellationTokenSource();
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
            _socket.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
            Started = false;
        }

        public async Task RunReceiveLoopAsync()
        {
            while (!_backgroundReceiveCTS.IsCancellationRequested)
            {
                var lengthData = await _socket.ReceiveAsync(2, _backgroundReceiveCTS.Token).ConfigureAwait(false);
                if (_backgroundReceiveCTS.IsCancellationRequested || lengthData.Length != 2)
                    break;

                var length = _numberEncoderService.DecodeNumber(lengthData);

                var packetData = await _socket.ReceiveAsync(length, _backgroundReceiveCTS.Token).ConfigureAwait(false);
                if (_backgroundReceiveCTS.IsCancellationRequested || packetData.Length != length)
                    break;

                var packet = _packetProcessActions.DecodeData(packetData);
                LogReceivedPacket(packet);

                _packetHandlingActions.EnqueuePacketForHandling(packet);
            }
        }

        public void CancelBackgroundReceiveLoop()
        {
            _backgroundReceiveCTS.Cancel();
        }

        public int Send(IPacket packet)
        {
            var sendTask = SendAsync(packet);
            return sendTask.Result;
        }

        public async Task<int> SendAsync(IPacket packet, int timeout = 1500)
        {
            LogSentPacket(packet, true);
            var bytesToSend = _packetProcessActions.EncodePacket(packet);
            using (var cts = new CancellationTokenSource(timeout))
                return await _socket.SendAsync(bytesToSend, cts.Token).ConfigureAwait(false);
        }

        public async Task<int> SendRawPacketAsync(IPacket packet, int timeout = 1500)
        {
            LogSentPacket(packet, false);
            var bytesToSend = _packetProcessActions.EncodeRawPacket(packet);
            using (var cts = new CancellationTokenSource(timeout))
                return await _socket.SendAsync(bytesToSend, cts.Token).ConfigureAwait(false);
        }

        [Conditional("DEBUG")]
        private void LogReceivedPacket(IPacket packet)
        {
            _loggerProvider.Logger.Log("RECV thread: Received             packet Family={0,-13} Action={1,-8} sz={2,-5} data={3}",
                Enum.GetName(typeof(PacketFamily), packet.Family),
                Enum.GetName(typeof(PacketAction), packet.Action),
                packet.Length,
                string.Join(":", packet.RawData.Select(b => $"{b:x2}")));
        }

        [Conditional("DEBUG")]
        private void LogSentPacket(IPacket packet, bool encoded)
        {
            _loggerProvider.Logger.Log("SEND thread: Processing       {0,-3} packet Family={1,-13} Action={2,-8} sz={3,-5} data={4}",
                    encoded ? "ENC" : "RAW",
                    Enum.GetName(typeof(PacketFamily), packet.Family),
                    Enum.GetName(typeof(PacketAction), packet.Action),
                    packet.Length,
                    string.Join(":", packet.RawData.Select(b => $"{b:x2}")));
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
                    CancelBackgroundReceiveLoop();
                    Disconnect();
                }

                _backgroundReceiveCTS.Dispose();
                _socket.Dispose();
            }
        }
    }
}
