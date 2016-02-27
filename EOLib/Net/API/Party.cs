// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Net.API
{
	public enum PartyRequestType
	{
		Join,
		Invite
	}

	public struct PartyMember
	{
		private readonly bool m_isFullData;
		/// <summary>
		/// Determines if full data is present. When false, only ID and PercentHealth are valid.
		/// </summary>
		public bool IsFullData { get { return m_isFullData; } }

		private readonly short m_id;
		private readonly bool m_isLeader;
		private readonly byte m_level;
		private byte m_pctHealth;
		private readonly string m_name;

		public short ID { get { return m_id; } }
		public bool IsLeader { get { return m_isLeader; } }
		public byte Level { get { return m_level; } }
		public byte PercentHealth { get { return m_pctHealth; } }
		public string Name { get { return m_name; } }

		internal PartyMember(OldPacket pkt, bool isFullData)
		{
			m_isFullData = isFullData;
			m_id = pkt.GetShort();
			if (!m_isFullData)
			{
				//if it isn't full data, only the id and health are being provide as part of an AGREE packet
				m_pctHealth = pkt.GetChar();
				m_isLeader = false;
				m_level = 0;
				m_name = "";
				return;
			}

			m_isLeader = pkt.GetChar() != 0;
			m_level = pkt.GetChar();
			m_pctHealth = pkt.GetChar();
			m_name = pkt.GetBreakString();
			m_name = char.ToUpper(m_name[0]) + m_name.Substring(1);
		}

		public void SetPercentHealth(byte percentHealth)
		{
			m_pctHealth = percentHealth;
		}
	}

	partial class PacketAPI
	{
		public delegate void PartyRequestEvent(PartyRequestType type, short playerID, string name);

		public event PartyRequestEvent OnPartyRequest;
		public event Action<List<PartyMember>> OnPartyDataRefresh;
		public event Action<PartyMember> OnPartyMemberJoin;
		public event Action<short> OnPartyMemberLeave;
		public event Action OnPartyClose;

		private void _createPartyMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Party, PacketAction.Request), _handlePartyRequest, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Party, PacketAction.Create), _handlePartyCreateList, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Party, PacketAction.List), _handlePartyCreateList, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Party, PacketAction.Agree), _handlePartyAgree, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Party, PacketAction.Add), _handlePartyAdd, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Party, PacketAction.Remove), _handlePartyRemove, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Party, PacketAction.Close), _handlePartyClose, true);
		}

		/// <summary>
		/// Send a party request to another player
		/// </summary>
		/// <param name="type">Either Join or Invite</param>
		/// <param name="otherCharID">ID of the other character</param>
		/// <returns>True on successful send operation, false otherwise</returns>
		public bool PartyRequest(PartyRequestType type, short otherCharID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			OldPacket pkt = new OldPacket(PacketFamily.Party, PacketAction.Request);
			pkt.AddChar((byte)type);
			pkt.AddShort(otherCharID);

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Accept another character's party request
		/// </summary>
		/// <param name="type">Join to join another player's party, invite to have them join yours</param>
		/// <param name="otherCharID">ID of the other character</param>
		/// <returns>True on successful send operation, false otherwise</returns>
		public bool PartyAcceptRequest(PartyRequestType type, short otherCharID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			OldPacket pkt = new OldPacket(PacketFamily.Party, PacketAction.Accept);
			pkt.AddChar((byte)type);
			pkt.AddShort(otherCharID);

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Remove a player from a party
		/// </summary>
		/// <param name="otherCharID">ID of the other character</param>
		/// <returns></returns>
		public bool PartyRemovePlayer(short otherCharID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			OldPacket pkt = new OldPacket(PacketFamily.Party, PacketAction.Remove);
			pkt.AddShort(otherCharID);

			return m_client.SendPacket(pkt);
		}

		public bool PartyListMembers()
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			return m_client.SendPacket(new OldPacket(PacketFamily.Party, PacketAction.Take));
		}

		//handles a request to join/invite from another player
		private void _handlePartyRequest(OldPacket pkt)
		{
			if (OnPartyRequest == null) return;
			PartyRequestType type = (PartyRequestType) pkt.GetChar();
			short playerID = pkt.GetShort();
			string name = pkt.GetEndString();
			name = char.ToUpper(name[0]) + name.Substring(1);
			OnPartyRequest(type, playerID, name);
		}

		//handles party_create and party_list packets
		//party_create should do some creation logic - party_list should only update the members info
		private void _handlePartyCreateList(OldPacket pkt)
		{
			if (OnPartyDataRefresh == null) return;
			List<PartyMember> members = new List<PartyMember>();
			while (pkt.ReadPos != pkt.Length)
				members.Add(new PartyMember(pkt, true));
			OnPartyDataRefresh(members);
		}

		//handles an HP update for party members
		private void _handlePartyAgree(OldPacket pkt)
		{
			if (OnPartyDataRefresh == null) return;
			List<PartyMember> members = new List<PartyMember>();
			while (pkt.ReadPos != pkt.Length)
				members.Add(new PartyMember(pkt, false));
			OnPartyDataRefresh(members);
		}

		private void _handlePartyAdd(OldPacket pkt)
		{
			if(OnPartyMemberJoin != null)
				OnPartyMemberJoin(new PartyMember(pkt, true));
		}

		//handles removing a player from a party (not this player)
		private void _handlePartyRemove(OldPacket pkt)
		{
			if (OnPartyMemberLeave != null)
				OnPartyMemberLeave(pkt.GetShort()); //id of character leaving
		}

		//closes a party on this end
		private void _handlePartyClose(OldPacket pkt)
		{
			//1 byte - 255
			if (OnPartyClose != null)
				OnPartyClose();
		}
	}
}
