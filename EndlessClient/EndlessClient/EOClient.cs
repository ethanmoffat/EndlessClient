using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

using EOLib;

namespace EndlessClient
{
	public class EOClient : AsyncClient, IDisposable
	{
		private delegate void PacketHandler(Packet reader);

		//I COULD just use tuple...but it is easier to type when I make a wrapper that basically is a tuple.
		private struct FamilyActionPair
		{
			public PacketFamily fam;
			public PacketAction act;

			public FamilyActionPair(PacketFamily family, PacketAction action)
			{
				fam = family;
				act = action;
			}
		}
		
		//this is a wrapper that serializes thread access to the handler method. This serialization can be overriden.
		private class LockedHandlerMethod
		{
			private readonly PacketHandler _handler;
			private readonly bool _override;
			private readonly bool _inGameOnly;

			public PacketHandler Handler
			{
				get
				{
					if(_inGameOnly && GameStates.PlayingTheGame != EOGame.Instance.State) //force ignore if the handler is an in-game only handler
						return p => { };
					if (_override) return _handler;
					lock (locker)
						return _handler;
				}
			}
			private static readonly object locker = new object();

			public LockedHandlerMethod(PacketHandler handler, bool inGameOnly = false, bool overrideLock = false)
			{
				_override = overrideLock;
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
					new FamilyActionPair(PacketFamily.Connection, PacketAction.Player),
					new LockedHandlerMethod(Handlers.Connection.PingResponse)
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
					new FamilyActionPair(PacketFamily.Login, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Login.LoginResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Agree), 
					new LockedHandlerMethod(Handlers.Players.PlayersAgree, true)
				},
				{
					new FamilyActionPair(PacketFamily.Refresh, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Refresh.RefreshReply, true)
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

		public new void Disconnect()
		{
			World.Instance.MainPlayer.Logout();
			base.Disconnect();
		}

		protected override void _handle(object state)
		{
			Packet pkt = (Packet)state;

			FamilyActionPair pair = new FamilyActionPair(pkt.Family, pkt.Action);
			if(handlers.ContainsKey(pair))
			{
				handlers[pair].Handler(pkt);
			}
		}

		public new void Dispose()
		{
			Handlers.Account.Cleanup();
			Handlers.Character.Cleanup();
			Handlers.Init.Cleanup();
			Handlers.Login.Cleanup();
			Handlers.Walk.Cleanup();
			Handlers.Welcome.Cleanup();

			base.Dispose();
		}
	}
}
