using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace EOLib.Net
{
	/// <summary>
	/// A single chunk of data (wraps around a byte array)
	/// </summary>
	public class DataChunk
	{
		public byte[] Data { get; set; }

		public DataChunk(int size = 128)
		{
			Data = new byte[size];
		}
	}

	public abstract class AsyncClient : IDisposable
	{
		private readonly object disposingLockObject = new object();

		private Socket m_sock;
		private EndPoint m_serverEndpoint;
		private bool m_disposing;
		private bool m_connectedAndInitialized;
		private AutoResetEvent m_sendLock;

		/// <summary>
		/// Returns a flag that is set when the Connect() method returns successfully.
		/// </summary>
		public bool ConnectedAndInitialized
		{
			get { return m_connectedAndInitialized && Connected; }
		}

		/// <summary>
		/// Polls the socket for the connection status. Retrieves connection status based on underlying socket.
		/// Sets ConnectedAndInitialized to 'false' if the socket cannot be polled.
		/// </summary>
		public bool Connected
		{
			get
			{
				try
				{
					bool c = !(m_sock.Poll(1000, SelectMode.SelectRead) && m_sock.Available == 0);
					if (!c && m_connectedAndInitialized)
					{
						Disconnect();
						m_connectedAndInitialized = false;
					}
					return c;
				}
				catch
				{
					return false;
				}
			}
		}

		//Set up socket to prepare for connection
		protected AsyncClient()
		{
			m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Connects to the server specified when the constructor was called
		/// </summary>
		/// <returns>True if successful, false otherwise</returns>
		public bool ConnectToServer(string ipOrHostname, int port)
		{
			if (m_connectedAndInitialized)
			{
				throw new InvalidOperationException("Client has already connected to the server. Disconnect first before connecting again.");
			}

			if (m_serverEndpoint == null)
			{
				IPAddress ip;
				if (!IPAddress.TryParse(ipOrHostname, out ip))
				{
					IPHostEntry entry = Dns.GetHostEntry(ipOrHostname);
					if (entry.AddressList.Length == 0)
						return false;

					ipOrHostname = entry.AddressList[0].ToString();
				}

				try
				{
					m_serverEndpoint = new IPEndPoint(IPAddress.Parse(ipOrHostname), port);
				}
				catch
				{
					return false;
				}
			}

			try
			{
				if (m_sock != null && m_sock.Connected)
				{
					m_connectedAndInitialized = true;
					return true;
				}

				if (m_sock == null)
				{
					return m_connectedAndInitialized = false;
				}

				if (m_sendLock != null)
				{
					m_sendLock.Close();
					m_sendLock = null;
				}

				m_sendLock = new AutoResetEvent(true);

				m_sock.Connect(m_serverEndpoint);
				m_connectedAndInitialized = true;

				if (Connected)
				{
					OnConnect();
				}
			}
			catch
			{
				m_connectedAndInitialized = false;
			}

			return m_connectedAndInitialized;
		}

		/// <summary>
		/// Provides for implementation-specific logic to be called on a successful socket connect() operation
		/// </summary>
		protected virtual void OnConnect()
		{
			DataChunk wrap = new DataChunk();
			m_sock.BeginReceive(wrap.Data, 0, wrap.Data.Length, SocketFlags.None, _recvCB, wrap);
		}

		/// <summary>
		/// Starts an asyncronous receive operation on the underlying socket
		/// </summary>
		protected void StartDataReceive(DataChunk buffer)
		{
			m_sock.BeginReceive(buffer.Data, 0, buffer.Data.Length, SocketFlags.None, _recvCB, buffer);
		}

		/// <summary>
		/// Yanks raw data out of the receive buffer for the underlying socket
		/// </summary>
		/// <returns>Number of bytes received</returns>
		protected int ReceiveRaw(ref byte[] rawData)
		{
			return m_sock.Receive(rawData, rawData.Length, SocketFlags.None);
		}

		/// <summary>
		/// Disconnects from the server and recreates the socket. 
		/// </summary>
		public virtual void Disconnect()
		{
			if (!m_connectedAndInitialized)
			{
				return;
			}

			m_connectedAndInitialized = false;
			if (m_sock != null)
			{
				m_sock.Shutdown(SocketShutdown.Both);
				//m_sock.Disconnect(false); //this seems to cause errors: a disconnected socket can only be reconnected asyncronously which is a pain in the ass
				m_sock.Close();
			}

			Socket newSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			m_sock = newSock;
		}

		//---------------------------------------
		// Send a packet to the server
		//---------------------------------------

		public bool SendPacket(Packet pkt)
		{
			if (!m_sendLock.WaitOne(Constants.ResponseTimeout)) //do one send at a time
				return false;

			byte[] toSend;
			OnSendData(pkt, out toSend);
			DataChunk wrap = new DataChunk(toSend.Length) {Data = toSend};
			try
			{
				m_sock.BeginSend(wrap.Data, 0, wrap.Data.Length, SocketFlags.None, _sendCB, wrap);
			}
			catch (SocketException)
			{
				//connection aborted by hardware errors produce a socketexception.
				return false;
			}
			return true;
		}

		/// <summary>
		/// Does optional processing of packet data before sending it to the server
		/// </summary>
		protected virtual void OnSendData(Packet pkt, out byte[] toSend)
		{
			toSend = pkt.Get();
		}

		/// <summary>
		/// Send unencrypted packet to server
		/// </summary>
		public bool SendRaw(Packet pkt)
		{
			m_sendLock.WaitOne();

			byte[] toSend;
			OnSendRawData(pkt, out toSend);
			DataChunk wrap = new DataChunk(toSend.Length) {Data = toSend};
			try
			{
				m_sock.BeginSend(wrap.Data, 0, wrap.Data.Length, SocketFlags.None, _sendCB, wrap);
			}
			catch (SocketException)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Does optional processing of raw packet data before sending it to the server
		/// </summary>
		protected virtual void OnSendRawData(Packet pkt, out byte[] toSend)
		{
			toSend = pkt.Get();
		}

		//-----------------------------------
		//send and receive callback functions
		//-----------------------------------

		private void _recvCB(IAsyncResult res)
		{
			lock (disposingLockObject)
				if (m_disposing)
					return;

			int bytes;
			DataChunk wrap;
			try
			{
				bytes = m_sock.EndReceive(res);
				wrap = (DataChunk) res.AsyncState;
			}
			catch
			{
				return;
			}

			if (bytes == 0)
			{
				Console.WriteLine("There was an error in the receive callback. Closing connection.");
				Disconnect();
				return;
			}

			OnReceiveData(wrap);
		}

		/// <summary>
		/// Processes the data chunk that was received from the server - MUST BE OVERRIDDEN WITH PROCESSING LOGIC
		/// </summary>
		protected abstract void OnReceiveData(DataChunk wrappedData);

		private void _sendCB(IAsyncResult res)
		{
			lock (disposingLockObject)
				if (m_disposing)
				{
					m_sendLock.Set();
					return;
				}

			int bytes = m_sock.EndSend(res);
			DataChunk wrap = (DataChunk) res.AsyncState;
			if (bytes != wrap.Data.Length)
			{
				m_sendLock.Set();
				throw new InvalidOperationException("There was an error sending the specified number of bytes to the server.");
			}

			m_sendLock.Set(); //send completed asyncronously. Allow pending sends to continue
		}

		//-----------------------------------
		//dispose method
		//-----------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;

			lock (disposingLockObject)
			{
				if (m_disposing)
					return;

				m_disposing = true;
			}

			if (m_sendLock != null)
			{
				m_sendLock.Set();
				m_sendLock.Close();
			}
			if (m_connectedAndInitialized)
				m_sock.Shutdown(SocketShutdown.Both);

			m_sock.Close();
			m_sock = null;
		}
	}
}
