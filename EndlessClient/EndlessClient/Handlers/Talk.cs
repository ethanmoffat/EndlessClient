using System;
using System.Threading;
using EOLib;
using Microsoft.Xna.Framework;

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
		/// <summary>
		/// sends all different types of Talk packets to server based on which chat type we're doing
		/// </summary>
		/// <param name="chatType">Which type of chat message is being sent</param>
		/// <param name="message">The message being sent</param>
		/// <param name="character">The character (required for TalkType.PM)</param>
		/// <returns></returns>
		public static bool Speak(TalkType chatType, string message, string character = null)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;
			
			Packet builder;
			switch (chatType)
			{
				case TalkType.Local:
					builder = new Packet(PacketFamily.Talk, PacketAction.Report);
					break;
				case TalkType.PM:
					builder = new Packet(PacketFamily.Talk, PacketAction.Tell);
					if (string.IsNullOrWhiteSpace(character))
						return false;
					builder.AddBreakString(character);
					break;
				default: throw new NotImplementedException();
			}

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

			World.Instance.ActiveMapRenderer.RenderChatMessage(TalkType.Local, fromPlayerID, message, ChatType.SpeechBubble);
		}

		/// <summary>
		/// Handler for the TALK_REPLY packet (sent in response to not-found for PMs sent from this end)
		/// </summary>
		public static void TalkReply(Packet pkt)
		{
			switch (pkt.GetShort())
			{
				//player is not found so a sys error needs to be displayed
				case 1: //TALK_NOTFOUND response
					string from = pkt.GetEndString();
					from = from.Substring(0, 1).ToUpper() + from.Substring(1).ToLower();
					EOGame.Instance.Hud.PrivatePlayerNotFound(from);
					break;
			}
		}

		/// <summary>
		/// Handler for the TALK_TELL packet (sent in response to PM messages)
		/// </summary>
		public static void TalkTell(Packet pkt)
		{
			string from = pkt.GetBreakString();
			from = from.Substring(0, 1).ToUpper() + from.Substring(1).ToLower();
			string message = pkt.GetBreakString();

			EOGame.Instance.Hud.AddChat(ChatTabs.Local, from, message, ChatType.Note, ChatColor.PM);
			ChatTabs tab = EOGame.Instance.Hud.GetPrivateChatTab(from);
			if(tab != ChatTabs.None)
				EOGame.Instance.Hud.AddChat(tab, from, message, ChatType.Note);
		}
	}
}
