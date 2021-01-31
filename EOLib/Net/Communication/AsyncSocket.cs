using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    [Flags]
    public enum ConnectResult
    {
        /// <summary>
        /// Connect succeeded
        /// </summary>
        Success,
        /// <summary>
        /// Endpoint is invalid (most likely null)
        /// </summary>
        InvalidEndpoint,
        /// <summary>
        /// Socket is invalid (most likely disposed)
        /// </summary>
        InvalidSocket,
        /// <summary>
        /// WinSock error code
        /// </summary>
        SocketError,
        /// <summary>
        /// Operation timed out
        /// </summary>
        Timeout,
        /// <summary>
        /// Socket is already connected
        /// </summary>
        AlreadyConnected
    }

    public class AsyncSocket : IAsyncSocket
    {
        private readonly Socket _socket;
        private bool _connected;

        public bool Connected => BlockingIsConnected();

        public AsyncSocket(AddressFamily family, SocketType type, ProtocolType protocol)
        {
            _socket = new Socket(family, type, protocol);
        }

        public Task<int> SendAsync(byte[] data, CancellationToken ct)
        {
            return Task.Run(() => BlockingSend(data), ct);
        }

        public Task<byte[]> ReceiveAsync(int bytes, CancellationToken ct)
        {
            return Task.Run(() => BlockingReceive(bytes), ct);
        }

        public Task<bool> CheckIsConnectedAsync(CancellationToken ct)
        {
            return Task.Run(() => BlockingIsConnected(), ct);
        }

        public Task<ConnectResult> ConnectAsync(EndPoint endPoint, CancellationToken ct)
        {
            return Task.Run(() => BlockingConnect(endPoint), ct);
        }

        public Task DisconnectAsync(CancellationToken ct)
        {
            return Task.Run(() => BlockingDisconnect(), ct);
        }

        private int BlockingSend(byte[] data)
        {
            try
            {
                return _socket.Send(data);
            }
            catch (SocketException)
            {
                return 0;
            }
        }

        private byte[] BlockingReceive(int bytes)
        {
            var ret = new byte[bytes];
            var numBytes = 0;

            do
            {
                var localBytes = new byte[bytes - numBytes];
                var startIndex = numBytes;

                try
                {
                    numBytes += _socket.Receive(localBytes, bytes - numBytes, SocketFlags.None);
                }
                catch (SocketException)
                {
                    return new byte[0];
                }
                catch (ObjectDisposedException)
                {
                    return new byte[0];
                }

                Array.Copy(localBytes, 0, ret, startIndex, numBytes - startIndex);
            } while (numBytes < bytes);

            return ret;
        }

        private bool BlockingIsConnected()
        {
            try
            {
                var pollResult = !_socket.Poll(1000, SelectMode.SelectRead);
                var dataAvailable = _socket.Available != 0;
                return _connected && (pollResult || dataAvailable);
            }
            catch (ObjectDisposedException)
            {
                _connected = false;
                return false;
            }
        }

        private ConnectResult BlockingConnect(EndPoint endPoint)
        {
            if (_connected)
                return ConnectResult.AlreadyConnected;

            ConnectResult result;
            try
            {
                _socket.Connect(endPoint);
                result = ConnectResult.Success;
                _connected = true;
            }
            catch(ArgumentNullException)
            {
                result = ConnectResult.InvalidEndpoint;
            }
            catch(SocketException sex)
            {
                result = ConnectResult.SocketError | (ConnectResult)sex.ErrorCode;
            }
            catch(ObjectDisposedException)
            {
                result = ConnectResult.InvalidSocket;
            }
            catch(InvalidOperationException)
            {
                result = ConnectResult.InvalidSocket;
            }

            return result;
        }

        private void BlockingDisconnect()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);

                if (_socket.Connected)
                    _socket.Disconnect(false);

                _socket.Close();
            }
            catch (ObjectDisposedException)
            {
                _connected = false;
            }
        }

        #region IDisposable

        ~AsyncSocket()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _socket.Dispose();
            }
        }

        #endregion
    }
}
