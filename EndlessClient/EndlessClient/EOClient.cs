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
		private Dictionary<FamilyActionPair, PacketHandler> handlers;
		
		public EOClient()
			: base()
		{
			handlers = new Dictionary<FamilyActionPair, PacketHandler>();

			handlers.Add(new FamilyActionPair(PacketFamily.Account, PacketAction.Reply), Handlers.Account.AccountResponse);
			handlers.Add(new FamilyActionPair(PacketFamily.Character, PacketAction.Player), Handlers.Character.CharacterPlayerResponse);
			handlers.Add(new FamilyActionPair(PacketFamily.Character, PacketAction.Reply), Handlers.Character.CharacterResponse);
			handlers.Add(new FamilyActionPair(PacketFamily.Connection, PacketAction.Player), Handlers.Connection.PingResponse);
			handlers.Add(new FamilyActionPair(PacketFamily.Init, PacketAction.Init), Handlers.Init.InitResponse);
			handlers.Add(new FamilyActionPair(PacketFamily.Login, PacketAction.Reply), Handlers.Login.LoginResponse);
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
				handlers[pair](pkt);
			}
		}

		public new void Dispose()
		{
			Handlers.Account.Cleanup();
			Handlers.Character.Cleanup();
			Handlers.Init.Cleanup();
			Handlers.Login.Cleanup();

			base.Dispose();
		}
	}
}
