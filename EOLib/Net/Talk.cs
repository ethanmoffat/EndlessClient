// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Net
{
	public enum TalkType
	{
		Admin,
		PM, //private message: !
		Local, //local chat: no prefix
		Global,//global chat: ~
		Guild, //guild chat: &
		Party, //party chat: '
		Announce, //global chat: @
		NPC, //npc is saying something
		Server //server is saying something
	}

	partial class PacketAPI
	{
		public delegate void ReceivePublicChatEvent(TalkType type, int playerID, string message);
		public delegate void ReceiveOtherChatEvent(TalkType type, string player, string message);
		public event ReceivePublicChatEvent OnPlayerChatByID; //chat event that should be shown in some kind of speech bubble and chat panel
		public event ReceiveOtherChatEvent OnPlayerChatByName; //chat event that should only be shown in the chat panel
		public event Action<string> OnPMRecipientNotFound;
		public event Action<string> OnMuted;

		private void _createTalkMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Player), _handleTalkPlayer, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Reply), _handleTalkReply, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Request), _handleTalkRequest, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Tell), _handleTalkTell, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Message), _handleTalkMessage, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Open), _handleTalkOpen, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Server), _handleTalkServer, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Admin), _handleTalkAdmin, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Announce), _handleTalkAnnounce, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Spec), _handleTalkSpec, true);
		}

		public bool Speak(TalkType chatType, string message, string character = null)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
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
						throw new ArgumentException("Unable to send a PM to invalid character!", "character");
					builder.AddBreakString(character);
					break;
				case TalkType.Global:
					builder = new Packet(PacketFamily.Talk, PacketAction.Message);
					break;
				case TalkType.Guild:
					builder = new Packet(PacketFamily.Talk, PacketAction.Request);
					break;
				case TalkType.Party:
					builder = new Packet(PacketFamily.Talk, PacketAction.Open);
					break;
				case TalkType.Admin:
					builder = new Packet(PacketFamily.Talk, PacketAction.Admin);
					break;
				case TalkType.Announce:
					builder = new Packet(PacketFamily.Talk, PacketAction.Announce);
					break;
				default: throw new NotImplementedException();
			}

			builder.AddString(message);

			return m_client.SendPacket(builder);
		}

		/// <summary>
		/// Handler for the TALK_PLAYER packet (sent for public chat messages)
		/// </summary>
		private void _handleTalkPlayer(Packet pkt)
		{
			if (OnPlayerChatByID == null) return;
			short fromPlayerID = pkt.GetShort();
			string message = pkt.GetEndString();

			OnPlayerChatByID(TalkType.Local, fromPlayerID, message);
		}

		/// <summary>
		/// Handler for the TALK_REPLY packet (sent in response to not-found for PMs sent from this end)
		/// </summary>
		private void _handleTalkReply(Packet pkt)
		{
			if (OnPMRecipientNotFound == null) return;

			switch (pkt.GetShort())
			{
				//player is not found so a sys error needs to be displayed
				case 1: //TALK_NOTFOUND response (no other members of this enum)
					string from = pkt.GetEndString();
					from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
					OnPMRecipientNotFound(from);
					break;
			}
		}

		/// <summary>
		/// Handler for the TALK_TELL packet (sent in response to PM messages)
		/// </summary>
		private void _handleTalkTell(Packet pkt)
		{
			if (OnPlayerChatByName == null) return;

			string from = pkt.GetBreakString();
			from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
			string message = pkt.GetBreakString();
			OnPlayerChatByName(TalkType.PM, from, message);
		}

		/// <summary>
		/// Handler for the TALK_MESSAGE packet (sent in response to global messages)
		/// </summary>
		private void _handleTalkMessage(Packet pkt)
		{
			if (OnPlayerChatByName == null) return;

			string from = pkt.GetBreakString();
			from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
			string message = pkt.GetBreakString();

			OnPlayerChatByName(TalkType.Global, from, message);
		}

		/// <summary>
		/// Handler for the TALK_REQUEST packet (sent in response to guild messages)
		/// </summary>
		private void _handleTalkRequest(Packet pkt)
		{
			if (OnPlayerChatByName == null) return;

			string from = pkt.GetBreakString();
			from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
			string message = pkt.GetBreakString();

			OnPlayerChatByName(TalkType.Guild, from, message);
		}

		/// <summary>
		/// Handler for the TALK_OPEN packet (sent in response to party messages)
		/// </summary>
		private void _handleTalkOpen(Packet pkt)
		{
			if (OnPlayerChatByName == null) return;

			short from = pkt.GetShort();
			string message = pkt.GetBreakString();

			OnPlayerChatByID(TalkType.Party, from, message);
		}

		private void _handleTalkServer(Packet pkt)
		{
			if (OnPlayerChatByName == null) return;

			string msg = pkt.GetEndString();
			OnPlayerChatByName(TalkType.Server, null, msg);
		}

		private void _handleTalkAdmin(Packet pkt)
		{
			if (OnPlayerChatByName == null) return;

			string name = pkt.GetBreakString();
			name = char.ToUpper(name[0]) + name.Substring(1);
			string msg = pkt.GetBreakString();

			OnPlayerChatByName(TalkType.Admin, name, msg);
		}

		private void _handleTalkAnnounce(Packet pkt)
		{
			if (OnPlayerChatByName == null) return;

			string name = pkt.GetBreakString();
			name = char.ToUpper(name[0]) + name.Substring(1);
			string msg = pkt.GetBreakString();

			OnPlayerChatByName(TalkType.Announce, name, msg);
		}

		private void _handleTalkSpec(Packet pkt)
		{
			if (OnMuted != null)
				OnMuted(pkt.GetEndString());
		}
	}
}
