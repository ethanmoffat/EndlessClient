using System;
using System.Collections;
using System.Collections.Generic;
using EOLib;

namespace EndlessClient
{
	public class EOClient : AsyncClient
	{
		private delegate void PacketHandler(Packet reader);

		//I COULD just use tuple...but it is easier to type when I make a wrapper that basically is a tuple.
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
		private class LockedHandlerMethod
		{
			private readonly PacketHandler _handler;
			private readonly bool _inGameOnly;

			public PacketHandler Handler
			{
				get
				{
					if(_inGameOnly && GameStates.PlayingTheGame != EOGame.Instance.State) //force ignore if the handler is an in-game only handler
						return p => { };
					lock (locker) return _handler;
				}
			}
			private static readonly object locker = new object();

			public LockedHandlerMethod(PacketHandler handler, bool inGameOnly = false)
			{
				_handler = handler;
				_inGameOnly = inGameOnly;
			}
		}
		private readonly Dictionary<FamilyActionPair, LockedHandlerMethod> handlers;
		
		public EOClient()
		{
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
		}

		public override void Disconnect()
		{
			World.Instance.MainPlayer.Logout();
			base.Disconnect();
		}

		protected override void _handle(object state)
		{
			Packet pkt = (Packet)state;

			string logOpt;
			FamilyActionPair pair = new FamilyActionPair(pkt.Family, pkt.Action);
			if(handlers.ContainsKey(pair))
			{
				logOpt = "  handled";
				handlers[pair].Handler(pkt);
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
				GetDataDisplayString(pkt.Data));
		}

		public override bool SendPacket(Packet pkt)
		{
			Logger.Log("SEND thread: Processing       ENC packet Family={0,-13} Action={1,-8} sz={2,-5} data={3}", 
				Enum.GetName(typeof(PacketFamily), pkt.Family),
				Enum.GetName(typeof(PacketAction), pkt.Action),
				pkt.Length,
				GetDataDisplayString(pkt.Data));
			return base.SendPacket(pkt);
		}

		public override bool SendRaw(Packet pkt)
		{
			Logger.Log("SEND thread: Processing       RAW packet Family={0,-13} Action={1,-8} sz={2,-5} data={3}",
				Enum.GetName(typeof(PacketFamily), pkt.Family),
				Enum.GetName(typeof(PacketAction), pkt.Action),
				pkt.Length,
				GetDataDisplayString(pkt.Data));
			return base.SendRaw(pkt);
		}

		private string GetDataDisplayString(byte[] data)
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
			base.Dispose(disposing);
			if (!disposing) return;

			Handlers.Account.Cleanup();
			Handlers.Character.Cleanup();
			Handlers.Init.Cleanup();
			Handlers.Login.Cleanup();
			Handlers.Walk.Cleanup();
			Handlers.Welcome.Cleanup();
		}
	}
}
