using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using EOLib;

namespace EndlessClient
{
	public class SocketDataWrapper
	{
		public enum DataReceiveState
		{
			ReadLen1,
			ReadLen2,
			ReadData,
			NoData
		}

// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
		public static int BUFFER_SIZE = 1;
// ReSharper restore FieldCanBeMadeReadOnly.Global
// ReSharper restore ConvertToConstant.Global

		public byte[] Data;
		public DataReceiveState State;
		public readonly byte[] RawLength;

		public SocketDataWrapper()
		{
			Data = new byte[BUFFER_SIZE];
			State = DataReceiveState.ReadLen1;
			RawLength = new byte[2];
		}
	}

	public abstract class AsyncClient : IDisposable
	{
		private static readonly object disposingLockObject = new object();

		private Socket m_sock;
		private EndPoint m_serverEndpoint;
		private bool m_disposing;
		private bool m_connectedAndInitialized;
		private AutoResetEvent m_sendLock;

		private ClientPacketProcessor m_packetProcessor;

		/// <summary>
		/// Returns a flag that is set when the Connect() method returns successfully.
		/// </summary>
		public bool ConnectedAndInitialized
		{
			get { return m_connectedAndInitialized; }
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
					if (!c && ConnectedAndInitialized)
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
				throw new Exception("Client has already connected to the server. Disconnect first before connecting again.");
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

				//this is stuff that would normally go in the constructor
				m_sendLock = new AutoResetEvent(true);
				m_packetProcessor = new ClientPacketProcessor(); //reset the packet processor to allow for new multis


				m_sock.Connect(m_serverEndpoint);
				SocketDataWrapper wrap = new SocketDataWrapper();
				m_sock.BeginReceive(wrap.Data, 0, wrap.Data.Length, SocketFlags.None, _recvCB, wrap);
				m_connectedAndInitialized = true;

				if (!Handlers.Init.Initialize() || !Handlers.Init.CanProceed)
				{
					//pop up some dialogs when this fails (see EOGame::TryConnectToServer)
					return (m_connectedAndInitialized = false);
				}

				m_packetProcessor.SetMulti(Handlers.Init.Data.emulti_d, Handlers.Init.Data.emulti_e);
				UpdateSequence(Handlers.Init.Data.seq_1*7 - 11 + Handlers.Init.Data.seq_2 - 2);
				World.Instance.MainPlayer.SetPlayerID(Handlers.Init.Data.clientID);

				//send confirmation of init data to server
				Packet confirm = new Packet(PacketFamily.Connection, PacketAction.Accept);
				confirm.AddShort(m_packetProcessor.SendMulti);
				confirm.AddShort(m_packetProcessor.RecvMulti);
				confirm.AddShort(Handlers.Init.Data.clientID);

				if (!SendPacket(confirm))
				{
					return (m_connectedAndInitialized = false);
				}
			}
			catch
			{
				m_connectedAndInitialized = false;
			}

			return m_connectedAndInitialized;
		}

		/// <summary>
		/// Disconnects from the server and recreates the socket. 
		/// </summary>
		public void Disconnect()
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
			byte[] toSend = pkt.Get();
			m_packetProcessor.Encode(ref toSend);

			byte[] data = new byte[toSend.Length + 2];
			byte[] len = Packet.EncodeNumber(toSend.Length, 2);

			Array.Copy(len, 0, data, 0, 2);
			Array.Copy(toSend, 0, data, 2, toSend.Length);

			if (!m_sendLock.WaitOne(Constants.ResponseTimeout)) //do one send at a time
				return false;

			SocketDataWrapper wrap = new SocketDataWrapper {Data = data};
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

		public bool SendRaw(Packet pkt)
		{
			pkt.WritePos = 0;
			pkt.AddShort((short) pkt.Length);

			m_sendLock.WaitOne();

			SocketDataWrapper wrap = new SocketDataWrapper {Data = pkt.Get()};
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

		//---------------------------------------
		// Overridable methods in derived classes
		// Currently just _handle
		//---------------------------------------

		protected abstract void _handle(object state);

		//-----------------------------------
		//send and receive callback functions
		//-----------------------------------

		private void _recvCB(IAsyncResult res)
		{
			lock (disposingLockObject)
				if (m_disposing)
					return;

			int bytes;
			SocketDataWrapper wrap;
			try
			{
				bytes = m_sock.EndReceive(res);
				wrap = (SocketDataWrapper) res.AsyncState;
			}
			catch
			{
				return;
			}

			if (bytes == 0)
			{
				wrap.State = SocketDataWrapper.DataReceiveState.NoData;
			}

			try
			{
				switch (wrap.State)
				{
					case SocketDataWrapper.DataReceiveState.ReadLen1:
					{
						wrap.RawLength[0] = wrap.Data[0];
						wrap.State = SocketDataWrapper.DataReceiveState.ReadLen2;
						wrap.Data = new byte[SocketDataWrapper.BUFFER_SIZE];
						m_sock.BeginReceive(wrap.Data, 0, wrap.Data.Length, SocketFlags.None, _recvCB, wrap);
						break;
					}
					case SocketDataWrapper.DataReceiveState.ReadLen2:
					{
						wrap.RawLength[1] = wrap.Data[0];
						wrap.State = SocketDataWrapper.DataReceiveState.ReadData;
						wrap.Data = new byte[Packet.DecodeNumber(wrap.RawLength)];
						m_sock.BeginReceive(wrap.Data, 0, wrap.Data.Length, SocketFlags.None, _recvCB, wrap);
						break;
					}
					case SocketDataWrapper.DataReceiveState.ReadData:
					{
						byte[] data = wrap.Data;
						m_packetProcessor.Decode(ref data);

						//This block handles receipt of file data that is transferred to the client.
						//It should make file transfer nuances pretty transparent to the client.
						//The header for files stored in a Packet type is always as follows: FAMILY_INIT, ACTION_INIT, (InitReply)
						//A 3-byte offset is found throughout the code that handles creating these files.
						Packet pkt = new Packet(data);
						if ((pkt.Family == PacketFamily.Init && pkt.Action == PacketAction.Init))
						{
							Handlers.InitReply reply = (Handlers.InitReply) pkt.GetChar();
							if (Handlers.Init.ExpectingFile)
							{
								int dataGrabbed = 0;

								int pktOffset = 0;
								for (; pktOffset < data.Length; ++pktOffset)
									if (data[pktOffset] == 0)
										break;

								do
								{
									byte[] fileBuffer = new byte[pkt.Length - pktOffset];
									int nextGrabbed = m_sock.Receive(fileBuffer);
									Array.Copy(fileBuffer, 0, data, dataGrabbed + 3, data.Length - (dataGrabbed + pktOffset));
									dataGrabbed += nextGrabbed;
								} while (dataGrabbed < pkt.Length - pktOffset);

								if (pktOffset > 3)
									data = data.SubArray(0, pkt.Length - (pktOffset - 3));

								data[2] = (byte) reply;
								//rewrite the InitReply with the correct value (retrieved with GetChar, server sends with GetByte for other reply types)
							}
						}

						ThreadPool.QueueUserWorkItem(_handle, new Packet(data));
						SocketDataWrapper newWrap = new SocketDataWrapper();
						m_sock.BeginReceive(newWrap.Data, 0, newWrap.Data.Length, SocketFlags.None, _recvCB, newWrap);
						break;
					}
					default:
					{
						Console.WriteLine("There was an error in the receive callback. Closing connection.");
						Disconnect();
						break;
					}
				}
			}
			catch (SocketException se)
			{
				//in the process of disconnecting
				Console.WriteLine("There was a SocketException with SocketErrorCode {0} in _recvCB", se.SocketErrorCode);
			}
		}

		private void _sendCB(IAsyncResult res)
		{
			lock (disposingLockObject)
				if (m_disposing)
				{
					m_sendLock.Set();
					return;
				}

			int bytes = m_sock.EndSend(res);
			SocketDataWrapper wrap = (SocketDataWrapper) res.AsyncState;
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

		public void UpdateSequence(int newVal)
		{
			m_packetProcessor.SequenceStart = newVal;
		}
	}
}
