// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using EOLib.Net.API;

namespace EOLib.Net
{
	public class EODataChunk : DataChunk
	{
		//endless online uses different states of receiving data
		//	Send 1 byte of length[0] (ReadLen1)
		//	Send 1 byte of length[1] (ReadLen2)
		//	Combine length[0]+length[1] to get total length
		//	Receive bytes until we have total length (ReadData)
		//NoData is an error state
		public enum DataReceiveState
		{
			ReadLen1,
			ReadLen2,
			ReadData,
			NoData
		}

		public const int BUFFER_SIZE = 1;

		public DataReceiveState State;
		public byte[] RawLength { get; private set; }

		public EODataChunk()
			: base(BUFFER_SIZE)
		{
			State = DataReceiveState.ReadLen1;
			RawLength = new byte[2];
		}
	}

	public class DataTransferEventArgs : EventArgs
	{
		public enum TransferType
		{
			Send,
			SendRaw,
			Recv
		}

		public PacketFamily PacketFamily { get; private set; }
		public PacketAction PacketAction { get; private set; }

		public TransferType Type { get; private set; }
		public byte[] RawByteData { get; private set; }

		public bool PacketHandled { get; set; }

		public string ByteDataHexString
		{
			get
			{
				string result = "";
				// ReSharper disable once LoopCanBeConvertedToQuery
				// ReSharper disable once ForCanBeConvertedToForeach
				for (int i = 0; i < RawByteData.Length; ++i)
					result += string.Format("{0}:", RawByteData[i].ToString("x2"));
				if (result.Length > 1)
					result = result.Substring(0, result.Length - 1);
				return result;
			}
		}

		public DataTransferEventArgs(TransferType type, PacketFamily family, PacketAction action, byte[] rawData)
		{
			Type = type;
			PacketFamily = family;
			PacketAction = action;
			RawByteData = rawData;
		}
	}

	public delegate void PacketHandler(Packet reader);
	public struct FamilyActionPair : IEqualityComparer
	{
		private readonly PacketFamily fam;
		private readonly PacketAction act;

		public FamilyActionPair(PacketFamily family, PacketAction action)
		{
			fam = family;
			act = action;
		}

		bool IEqualityComparer.Equals(object obj1, object obj2)
		{
			if (!(obj1 is FamilyActionPair) || !(obj2 is FamilyActionPair))
				return false;

			FamilyActionPair fap1 = (FamilyActionPair)obj1, fap2 = (FamilyActionPair)obj2;
			return fap1.fam == fap2.fam && fap1.act == fap2.act;
		}

		public int GetHashCode(object obj)
		{
			if (!(obj is FamilyActionPair)) return 0;

			FamilyActionPair fap /*lol*/ = (FamilyActionPair)obj;

			return (int)fap.fam << 8 & (byte)fap.act;
		}
	}

	internal class PacketHandlerInvoker
	{
		private readonly PacketHandler _handler;
		private readonly bool _inGameOnly;

		public PacketHandlerInvoker(PacketHandler handler, bool inGameOnly = false)
		{
			_handler = handler;
			_inGameOnly = inGameOnly;
		}

		public void InvokeHandler(Packet pkt, bool isInGame)
		{ //force ignore if the handler is an in-game only handler
			if (_inGameOnly && !isInGame)
				return;
			_handler(pkt);
		}
	}

	public class EOClient : ClientBase
	{
		private readonly Dictionary<FamilyActionPair, PacketHandlerInvoker> m_handlers;

		private readonly IPacketProcessorActions _packetProcessActions;

		public event Action<DataTransferEventArgs> EventSendData;
		public event Action<DataTransferEventArgs> EventReceiveData;
		public event Action EventDisconnect;

		internal bool IsInGame { private get; set; }

		//both of these are only used by the API
		/// <summary>
		/// Set to 'true' when a file is requested. Changes handling for received data
		/// </summary>
		internal bool ExpectingFile { get; set; }
		/// <summary>
		/// Set to 'true' when online player list is requested. Changes handling for received data
		/// </summary>
		internal bool ExpectingPlayerList { get; set; }

		public EOClient()
		{
			m_handlers = new Dictionary<FamilyActionPair, PacketHandlerInvoker>(128);

			//note: all this stuff should be dependency injected into an IOC container. I don't have one, so its done here manually.
			_packetProcessActions = new PacketProcessActions(new SequenceRepository(), new PacketEncoderRepository());
		}

		public void SetInitData(InitData data)
		{
			_packetProcessActions.SetInitialSequenceNumber(data.seq_1, data.seq_2);
			_packetProcessActions.SetEncodeMultiples(data.emulti_d, data.emulti_e);
		}

		public void UpdateSequence(int seq1, int seq2)
		{
			_packetProcessActions.SetUpdatedBaseSequenceNumber(seq1, seq2);
		}

		protected override void OnConnect()
		{
			EODataChunk wrap = new EODataChunk();
			StartDataReceive(wrap);
		}

		protected override void OnSendData(Packet pkt, out byte[] toSend)
		{
			//for debugging: sometimes the server is getting PACKET_INTERNAL from this client
			//I'm not sure why and it happens randomly so this check will allow me to examine
			//	packet contents and the call stack to figure it out
			if (pkt.Family == PacketFamily.Internal)
				throw new ArgumentException("This is an invalid packet!");

			toSend = _packetProcessActions.EncodePacket(pkt);

			//at this point, toSend should be 3 or 4 bytes longer than the original packet data
			//this includes 2 bytes of len, 1 or 2 bytes of seq, and then packet payload

			if (EventSendData != null)
			{
				DataTransferEventArgs dte = new DataTransferEventArgs(DataTransferEventArgs.TransferType.Send, pkt.Family, pkt.Action, pkt.Data);
				EventSendData(dte);
			}
		}

		protected override void OnSendRawData(Packet pkt, out byte[] toSend)
		{
			if (EventSendData != null)
			{
				DataTransferEventArgs dte = new DataTransferEventArgs(DataTransferEventArgs.TransferType.SendRaw, pkt.Family, pkt.Action, pkt.Data);
				EventSendData(dte);
			}

			pkt.WritePos = 0;
			pkt.AddShort((short)pkt.Length);
			toSend = pkt.Get();
		}

		protected override void OnReceiveData(DataChunk state)
		{
			EODataChunk wrap = (EODataChunk)state;
			if (wrap.Data.Length == 0)
				wrap.State = EODataChunk.DataReceiveState.NoData;
			try
			{
				switch (wrap.State)
				{
					case EODataChunk.DataReceiveState.ReadLen1:
						{
							wrap.RawLength[0] = wrap.Data[0];
							wrap.State = EODataChunk.DataReceiveState.ReadLen2;
							wrap.Data = new byte[EODataChunk.BUFFER_SIZE];
							StartDataReceive(wrap);
							break;
						}
					case EODataChunk.DataReceiveState.ReadLen2:
						{
							wrap.RawLength[1] = wrap.Data[0];
							wrap.State = EODataChunk.DataReceiveState.ReadData;
							wrap.Data = new byte[Packet.DecodeNumber(wrap.RawLength)];
							StartDataReceive(wrap);
							break;
						}
					case EODataChunk.DataReceiveState.ReadData:
						{
							var pkt = _packetProcessActions.DecodeData(wrap.Data);

							//This block handles receipt of file data that is transferred to the client.
							//It should make file transfer nuances pretty transparent to the client.
							//The header for files stored in a Packet type is always as follows: FAMILY_INIT, ACTION_INIT, (InitReply)
							//A 3-byte offset is found throughout the code that handles creating these files.

							//INIT_INIT packet! check to see if expecting a file or player list
							if (pkt.Family == PacketFamily.Init && pkt.Action == PacketAction.Init)
							{
								byte[] data = pkt.Get();
								byte reply = pkt.GetChar();

								if (ExpectingFile || reply == (byte)InitReply.INIT_MAP_MUTATION) //handle the map mutation: should work with the byte/char weirdness
								{
									int dataGrabbed = 0;

									//find first zero byte

									int pktOffset = 0;
									for (; pktOffset < data.Length; ++pktOffset)
										if (data[pktOffset] == 0)
											break;

									//continue receiving until we have grabbed enough data to fill the allocated packet buffer
									do
									{
										byte[] fileBuffer = new byte[pkt.Length - pktOffset];
										int nextGrabbed = ReceiveRaw(ref fileBuffer);
										Array.Copy(fileBuffer, 0, data, dataGrabbed + 3, data.Length - (dataGrabbed + pktOffset));
										dataGrabbed += nextGrabbed;
									} while (dataGrabbed < pkt.Length - pktOffset);

									if (pktOffset > 3)
										data = data.SubArray(0, pkt.Length - (pktOffset - 3));

									//rewrite the InitReply with the correct value (retrieved with GetChar, server sends with GetByte for other reply types)
									data[2] = reply;
								}
								else if (ExpectingPlayerList)
								{
									//online list sends a char... rewrite it with a byte so it is parsed correctly.
									data[2] = reply;
								}

								pkt = new Packet(data);
							}

							_handlePacket(pkt);
							var newWrap = new EODataChunk();
							StartDataReceive(newWrap);
							break;
						}
					default:
						{
							Console.WriteLine("ERROR: Invalid data wrapper state in _recvCB (should be ReadLen1, ReadLen2, or ReadData). Closing connection.");
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

		private void _handlePacket(object state)
		{
			Packet pkt = (Packet) state;
			FamilyActionPair pair = new FamilyActionPair(pkt.Family, pkt.Action);
			bool handled = false;
			if (m_handlers.ContainsKey(pair))
			{
				handled = true;
				m_handlers[pair].InvokeHandler(pkt, IsInGame);
			}

			if (EventReceiveData != null)
			{
				DataTransferEventArgs dte = new DataTransferEventArgs(DataTransferEventArgs.TransferType.Recv, pkt.Family, pkt.Action, pkt.Data)
				{
					PacketHandled = handled
				};
				EventReceiveData(dte);
			}
		}

		public void AddPacketHandler(FamilyActionPair key, PacketHandler handlerFunction, bool inGameOnly)
		{
			if (m_handlers.ContainsKey(key))
				m_handlers[key] = new PacketHandlerInvoker(handlerFunction, inGameOnly);
			else
				m_handlers.Add(key, new PacketHandlerInvoker(handlerFunction, inGameOnly));
		}

		public override void Disconnect()
		{
			if (EventDisconnect != null)
				EventDisconnect();

			IsInGame = false;

			base.Disconnect();
		}
	}
}
