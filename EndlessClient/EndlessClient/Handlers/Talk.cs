using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;

namespace EndlessClient.Handlers
{
	public enum TalkType
	{
		Admin,
		PM,
		Local,
		Global,
		Guild,
		Party,
		System
	}

	public static class Talk //rename to packet family
	{
		//sends all different types of Talk packets to server based on which chat type we're doing
		public static bool Speak(TalkType chatType, string message)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;
			
			PacketAction action;
			switch (chatType)
			{
				case TalkType.Local: action = PacketAction.Report; break;
				default: throw new NotImplementedException();
			}

			Packet builder = new Packet(PacketFamily.Talk, action);
			builder.AddString(message);

			return client.SendPacket(builder);
		}

		/// <summary>
		/// Handler for the TALK_PLAYER packet (sent for public chat messages)
		/// </summary>
		public static void TalkPlayer(Packet pkt)
		{
			short fromPlayerID = pkt.GetShort();
			string message = pkt.GetEndString();

			World.Instance.ActiveMapRenderer.RenderChatMessage(TalkType.Local, fromPlayerID, message);
		}
	}
}
