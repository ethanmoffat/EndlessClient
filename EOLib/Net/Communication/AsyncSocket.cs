// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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

		public AsyncSocket(AddressFamily family, SocketType type, ProtocolType protocol)
		{
			_socket = new Socket(family, type, protocol);
		}

		public async Task<int> SendAsync(byte[] data, CancellationToken ct)
		{
			var task = Task.Run(() => BlockingSend(data), ct);
			
			await task;
			return task.Result;
		}

		public async Task<byte[]> ReceiveAsync(int bytes, CancellationToken ct)
		{
			var task = Task.Run(() => BlockingReceive(bytes), ct);

			await task;
			return task.Result;
		}

		public async Task<bool> CheckIsConnectedAsync(CancellationToken ct)
		{
			var task = Task.Run(() => BlockingIsConnected(), ct);

			await task;
			return task.Result;
		}

		public async Task<ConnectResult> ConnectAsync(EndPoint endPoint, CancellationToken ct)
		{
			var task = Task.Run(() => BlockingConnect(endPoint), ct);

			await task;
			return task.Result;
		}

		public async Task DisconnectAsync(CancellationToken ct)
		{
			var task = Task.Run(() => BlockingDisconnect(), ct);
			
			await task;
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
				var localBytes = new byte[bytes];
				var startIndex = numBytes;

				try
				{
					numBytes += _socket.Receive(localBytes, bytes, SocketFlags.None);
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
			catch(ObjectDisposedException)
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
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Disconnect(false);
			_socket.Close();
		}

		#region IDisposable

		~AsyncSocket()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
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
