using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using EOLib;

namespace EndlessClient
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
	public partial class EOClient : AsyncClient
	{
		private delegate void PacketHandler(Packet reader);
		private struct FamilyActionPair : IEqualityComparer
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

				FamilyActionPair fap1 = (FamilyActionPair) obj1, fap2 = (FamilyActionPair) obj2;
				return fap1.fam == fap2.fam && fap1.act == fap2.act;
			}

			public int GetHashCode(object obj)
			{
				if (!(obj is FamilyActionPair)) return 0;

				FamilyActionPair fap /*lol*/ = (FamilyActionPair) obj;

				return (int) fap.fam << 8 & (byte) fap.act;
			}
		}
		
		//this is a wrapper that serializes thread access to the handler method. This serialization can be overriden.
		private sealed class LockedHandlerMethod : IDisposable
		{
			private readonly PacketHandler _handler;
			private readonly bool _inGameOnly;
			private AutoResetEvent _lock;
			private bool m_disposed;

			public LockedHandlerMethod(PacketHandler handler, bool inGameOnly = false)
			{
				_handler = handler;
				_inGameOnly = inGameOnly;
				_lock = new AutoResetEvent(true);
			}

			public void InvokeHandler(Packet pkt)
			{ //force ignore if the handler is an in-game only handler
				if (_inGameOnly && GameStates.PlayingTheGame != EOGame.Instance.State)
					return;

				if (m_disposed || _lock == null)
					return;
				_lock.WaitOne(Constants.ResponseTimeout);
				if (m_disposed)
					return;
				_handler(pkt);
				if(_lock != null)
					_lock.Set();
			}

			public void Dispose()
			{
				if (m_disposed || _lock == null)
					return;
				_lock.WaitOne();
				m_disposed = true;
				_lock.Dispose();
				_lock = null;
			}
		}
		private readonly Dictionary<FamilyActionPair, LockedHandlerMethod> handlers;

		private ClientPacketProcessor m_packetProcessor;

		public EOClient()
		{
			#region HANDLERS
			handlers = new Dictionary<FamilyActionPair, LockedHandlerMethod>
			{
				{
					new FamilyActionPair(PacketFamily.Account, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Account.AccountResponse)
				},
				{
					new FamilyActionPair(PacketFamily.AdminInteract, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.AdminInteract.AdminShow)
				},
				{
					new FamilyActionPair(PacketFamily.AdminInteract, PacketAction.Remove),
					new LockedHandlerMethod(Handlers.AdminInteract.AdminHide)
				},
				{
					new FamilyActionPair(PacketFamily.Appear, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.NPCPackets.AppearReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Attack, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Attack.AttackPlayerResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Avatar, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Avatar.AvatarAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.Avatar, PacketAction.Remove),
					new LockedHandlerMethod(Handlers.Avatar.AvatarRemove, true)
				},
				{
					new FamilyActionPair(PacketFamily.Bank, PacketAction.Open), 
					new LockedHandlerMethod(Handlers.Bank.BankOpenReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Bank, PacketAction.Reply), 
					new LockedHandlerMethod(Handlers.Bank.BankReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Character, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Character.CharacterPlayerResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Character, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Character.CharacterResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Chest, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Chest.ChestAgreeResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Chest, PacketAction.Get),
					new LockedHandlerMethod(Handlers.Chest.ChestGetResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Chest, PacketAction.Open),
					new LockedHandlerMethod(Handlers.Chest.ChestOpenResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Chest, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Chest.ChestReply)
				},
				{
					new FamilyActionPair(PacketFamily.Connection, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Connection.PingResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Door, PacketAction.Open),
					new LockedHandlerMethod(Handlers.Door.DoorOpenResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Effect, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Effect.EffectPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Emote, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Emote.EmotePlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Face, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Face.FacePlayerResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Init, PacketAction.Init),
					new LockedHandlerMethod(Handlers.Init.InitResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Add),
					new LockedHandlerMethod(Handlers.Item.ItemAddResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Drop),
					new LockedHandlerMethod(Handlers.Item.ItemDropResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Get),
					new LockedHandlerMethod(Handlers.Item.ItemGetResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Junk),
					new LockedHandlerMethod(Handlers.Item.ItemJunkResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Remove),
					new LockedHandlerMethod(Handlers.Item.ItemRemoveResponse, true)
				},
				{
					new FamilyActionPair(PacketFamily.Item, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Item.ItemReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Buy),
					new LockedHandlerMethod(Handlers.Locker.LockerBuy, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Get),
					new LockedHandlerMethod(Handlers.Locker.LockerGet, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Open),
					new LockedHandlerMethod(Handlers.Locker.LockerOpen, true)
				},
				{
					new FamilyActionPair(PacketFamily.Locker, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Locker.LockerReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Login, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Login.LoginResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Message, PacketAction.Pong), 
					new LockedHandlerMethod(Handlers.Message.Pong)
				},
				{
					new FamilyActionPair(PacketFamily.NPC, PacketAction.Accept),
					new LockedHandlerMethod(Handlers.NPCPackets.NPCAccept, true)
				},
				{
					new FamilyActionPair(PacketFamily.NPC, PacketAction.Player),
					new LockedHandlerMethod(Handlers.NPCPackets.NPCPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.NPC, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.NPCPackets.NPCReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.NPC, PacketAction.Spec),
					new LockedHandlerMethod(Handlers.NPCPackets.NPCSpec, true)
				},
				{
					new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Paperdoll.PaperdollAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Remove),
					new LockedHandlerMethod(Handlers.Paperdoll.PaperdollRemove, true)
				},
				{
					new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Paperdoll.PaperdollReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Players.PlayersAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Ping),
					new LockedHandlerMethod(Handlers.Players.PlayersPing, true)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Pong),
					new LockedHandlerMethod(Handlers.Players.PlayersPong, true)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Net3),
					new LockedHandlerMethod(Handlers.Players.PlayersNet3, true)
				},
				{
					new FamilyActionPair(PacketFamily.Recover, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Recover.RecoverAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.Recover, PacketAction.List),
					new LockedHandlerMethod(Handlers.Recover.RecoverList, true)
				},
				{
					new FamilyActionPair(PacketFamily.Recover, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Recover.RecoverPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Recover, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Recover.RecoverReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Refresh, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Refresh.RefreshReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Buy),
					new LockedHandlerMethod(Handlers.Shop.ShopBuy, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Create),
					new LockedHandlerMethod(Handlers.Shop.ShopCreate, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Open),
					new LockedHandlerMethod(Handlers.Shop.ShopOpen, true)
				},
				{
					new FamilyActionPair(PacketFamily.Shop, PacketAction.Sell),
					new LockedHandlerMethod(Handlers.Shop.ShopSell, true)
				},
				{
					new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Player),
					new LockedHandlerMethod(Handlers.StatSkill.StatSkillPlayer, true)
				},
				//TALK PACKETS
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Message),
					new LockedHandlerMethod(Handlers.Talk.TalkMessage, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Talk.TalkPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Talk.TalkReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Request),
					new LockedHandlerMethod(Handlers.Talk.TalkRequest, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Server),
					new LockedHandlerMethod(Handlers.Talk.TalkServer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Tell),
					new LockedHandlerMethod(Handlers.Talk.TalkTell, true)
				},
				//
				{
					new FamilyActionPair(PacketFamily.Walk, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Walk.WalkReply, true)
				},
				{
					new FamilyActionPair(PacketFamily.Walk, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Walk.WalkPlayer, true)
				},
				{
					new FamilyActionPair(PacketFamily.Warp, PacketAction.Agree),
					new LockedHandlerMethod(Handlers.Warp.WarpAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.Warp, PacketAction.Request),
					new LockedHandlerMethod(Handlers.Warp.WarpRequest, true)
				},
				{
					new FamilyActionPair(PacketFamily.Welcome, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Welcome.WelcomeResponse)
				}
			};
			#endregion
		}

		public override void Disconnect()
		{
			World.Instance.MainPlayer.Logout();
			base.Disconnect();
		}

		public void UpdateSequence(int newVal)
		{
			m_packetProcessor.SequenceStart = newVal;
		}

		protected override void OnConnect()
		{
			EODataChunk wrap = new EODataChunk();
			StartDataReceive(wrap);

			m_packetProcessor = new ClientPacketProcessor(); //reset the packet processor to allow for new multis

			if (!Handlers.Init.Initialize() || !Handlers.Init.CanProceed)
			{
				//pop up some dialogs when this fails (see EOGame::TryConnectToServer)
				m_connectedAndInitialized = false;
				return;
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
				m_connectedAndInitialized = false;
			}
		}

		protected override void OnSendData(Packet pkt, out byte[] toSend)
		{
			//for debugging: sometimes the server is getting PACKET_INTERNAL from this client
			//I'm not sure why and it happens randomly so this check will allow me to examine
			//	packet contents and the call stack to figure it out
			if (pkt.Family == PacketFamily.Internal)
				throw new ArgumentException("This is an invalid packet!");

			//encode the packet bytes (also prepends seq number: 2 bytes)
			byte[] packetBytes = pkt.Get();
			m_packetProcessor.Encode(ref packetBytes);

			//prepend the 2 bytes of length data to the packet data
			byte[] len = Packet.EncodeNumber(packetBytes.Length, 2);

			toSend = new byte[packetBytes.Length + 2];
			Array.Copy(len, 0, toSend, 0, 2);
			Array.Copy(packetBytes, 0, toSend, 2, packetBytes.Length);

			//at this point, toSend should be 4 bytes longer than the original packet data
			//this includes 2 bytes of len, 2 bytes of seq, and then packet payload

			Logger.Log("SEND thread: Processing       ENC packet Family={0,-13} Action={1,-8} sz={2,-5} data={3}",
				Enum.GetName(typeof(PacketFamily), pkt.Family),
				Enum.GetName(typeof(PacketAction), pkt.Action),
				pkt.Length,
				_convertDataToHexString(pkt.Data));
		}

		protected override void OnSendRawData(Packet pkt, out byte[] toSend)
		{
			pkt.WritePos = 0;
			pkt.AddShort((short)pkt.Length);
			toSend = pkt.Get();

			Logger.Log("SEND thread: Processing       RAW packet Family={0,-13} Action={1,-8} sz={2,-5} data={3}",
				Enum.GetName(typeof(PacketFamily), pkt.Family),
				Enum.GetName(typeof(PacketAction), pkt.Action),
				pkt.Length,
				_convertDataToHexString(pkt.Data));
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
							byte[] data = new byte[wrap.Data.Length];
							Array.Copy(wrap.Data, data, data.Length);
							m_packetProcessor.Decode(ref data);

							//This block handles receipt of file data that is transferred to the client.
							//It should make file transfer nuances pretty transparent to the client.
							//The header for files stored in a Packet type is always as follows: FAMILY_INIT, ACTION_INIT, (InitReply)
							//A 3-byte offset is found throughout the code that handles creating these files.
							Packet pkt = new Packet(data);
							if ((pkt.Family == PacketFamily.Init && pkt.Action == PacketAction.Init))
							{
								Handlers.InitReply reply = (Handlers.InitReply)pkt.GetChar();
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
										int nextGrabbed = ReceiveRaw(ref fileBuffer);
										Array.Copy(fileBuffer, 0, data, dataGrabbed + 3, data.Length - (dataGrabbed + pktOffset));
										dataGrabbed += nextGrabbed;
									} while (dataGrabbed < pkt.Length - pktOffset);

									if (pktOffset > 3)
										data = data.SubArray(0, pkt.Length - (pktOffset - 3));

									//rewrite the InitReply with the correct value (retrieved with GetChar, server sends with GetByte for other reply types)
									data[2] = (byte)reply;
								}
								else if (Handlers.Init.ExpectingPlayerList)
								{
									//online list sends a char... rewrite it with a byte so it is parsed correctly.
									data[2] = (byte)reply;
								}
							}

							ThreadPool.QueueUserWorkItem(_handlePacket, new Packet(data));
							EODataChunk newWrap = new EODataChunk();
							StartDataReceive(newWrap);
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

		private void _handlePacket(object state)
		{
			Packet pkt = (Packet) state;
			string logOpt;
			FamilyActionPair pair = new FamilyActionPair(pkt.Family, pkt.Action);
			if (handlers.ContainsKey(pair))
			{
				logOpt = "  handled";
				handlers[pair].InvokeHandler(pkt);
			}
			else
			{
				logOpt = "UNHANDLED";
			}

			Logger.Log("RECV thread: Processing {0} packet Family={1,-13} Action={2,-8} sz={3,-5} data={4}",
				logOpt,
				Enum.GetName(typeof(PacketFamily), pkt.Family),
				Enum.GetName(typeof(PacketAction), pkt.Action),
				pkt.Length,
				_convertDataToHexString(pkt.Data));
		}

		private string _convertDataToHexString(byte[] data)
		{
			//This will log a string of data that will be usable by the PacketDecoder utility. colon-delimited 2-character hex values.
			string result = "";
// ReSharper disable once LoopCanBeConvertedToQuery
// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < data.Length; ++i)
				result += string.Format("{0}:", data[i].ToString("x2"));
			if (result.Length > 1)
				result = result.Substring(0, result.Length - 1);
			return result;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Handlers.Account.Cleanup();
				Handlers.Character.Cleanup();
				Handlers.Init.Cleanup();
				Handlers.Login.Cleanup();
				Handlers.Walk.Cleanup();
				Handlers.Welcome.Cleanup();

				foreach(LockedHandlerMethod method in handlers.Values)
					method.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
