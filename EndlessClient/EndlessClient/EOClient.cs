using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using EOLib;

namespace EndlessClient
{
	public class EOClient : AsyncClient, IDisposable
	{
		private delegate void PacketHandler(Packet reader);

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
			private PacketHandler _handler;
			private bool _override;

			public PacketHandler Handler
			{
				get
				{
					if (!_override)
						lock (locker)
							return _handler;
					else
						return _handler;
				}
			}
			private static readonly object locker = new object();

			public LockedHandlerMethod(PacketHandler handler, bool overrideLock = false)
			{
				_override = overrideLock;
				_handler = handler;
			}
		}
		private Dictionary<FamilyActionPair, LockedHandlerMethod> handlers;
		
		public EOClient()
			: base()
		{
			handlers = new Dictionary<FamilyActionPair, LockedHandlerMethod>
			{
				{
					new FamilyActionPair(PacketFamily.Account, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Account.AccountResponse)
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
					new FamilyActionPair(PacketFamily.Init, PacketAction.Init),
					new LockedHandlerMethod(Handlers.Init.InitResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Login, PacketAction.Reply),
					new LockedHandlerMethod(Handlers.Login.LoginResponse)
				},
				{
					new FamilyActionPair(PacketFamily.Players, PacketAction.Agree), 
					new LockedHandlerMethod(Handlers.Players.PlayersAgree)
				},
				{
					new FamilyActionPair(PacketFamily.Talk, PacketAction.Player),
 					new LockedHandlerMethod(Handlers.Talk.TalkPlayer)
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
			Handlers.Welcome.Cleanup();

			base.Dispose();
		}
	}
}
