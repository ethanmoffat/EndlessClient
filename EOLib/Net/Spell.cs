// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib.Data;

namespace EOLib.Net
{
	public struct GroupSpellTarget
	{
		private readonly short _partyMemberID, _partyMemberPercentHealth, _partyMemberHP;

		public short MemberID { get { return _partyMemberID; } }
		public short MemberPercentHealth { get { return _partyMemberPercentHealth; } }
		public short MemberHP { get { return _partyMemberHP; } }

		internal GroupSpellTarget(Packet pkt)
		{
			_partyMemberID = pkt.GetShort();
			_partyMemberPercentHealth = pkt.GetChar();
			_partyMemberHP = pkt.GetShort();
		}
	}

	#region event delegates

	public delegate void OtherPlayerStartCastSpellEvent(short fromPlayerID, short spellID);
	public delegate void OtherPlayerCastSpellSelfEvent(short fromPlayerID, short spellID, int spellHP, byte percentHealth);
	public delegate void CastSpellSelfEvent(short fromPlayerID, short spellID, int spellHP, byte percentHealth, short hp, short tp);

	public delegate void CastSpellOtherEvent(
		short targetPlayerId, short sourcePlayerId, EODirection sourcePlayerDirection, 
		short spellId, int recoveredHp, byte targetPercentHealth, short targetPlayerCurrentHP = -1);

	public delegate void CastSpellGroupEvent(short spellID, short fromPlayerID, short fromPlayerTP, short spellHPGain, List<GroupSpellTarget> spellTargets);

	#endregion

	partial class PacketAPI
	{
		#region public events

		public event OtherPlayerStartCastSpellEvent OnOtherPlayerStartCastSpell;
		public event OtherPlayerCastSpellSelfEvent OnOtherPlayerCastSpellSelf;
		public event CastSpellSelfEvent OnCastSpellSelf;
		public event CastSpellOtherEvent OnCastSpellTargetOther;
		public event CastSpellGroupEvent OnCastSpellTargetGroup;

		#endregion

		#region initialization

		private void _createSpellMembers()
		{
			//note: see CAST_REPLY handler in _handleNPCReply for NPCs taking damage from a spell. handler is almost identical
			//		see CAST_ACCPT handler for leveling up off NPC kill via spell
			//		see CAST_SPEC  handler for regular NPC death via spell
			//other note: Spell attacks for PK are not supported yet
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Spell, PacketAction.Request), _handleSpellRequest, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Spell, PacketAction.TargetSelf), _handleSpellTargetSelf, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Spell, PacketAction.TargetOther), _handleSpellTargetOther, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Spell, PacketAction.TargetGroup), _handleSpellTargetGroup, true);
		}

		#endregion

		#region public API

		public bool PrepareCastSpell(short spellID)
		{
			if (spellID < 0) return false; //integer overflow resulted in negative number - server expects ushort

			if (!Initialized || !m_client.ConnectedAndInitialized) return false;

			Packet pkt = new Packet(PacketFamily.Spell, PacketAction.Request);
			pkt.AddShort(spellID);
			pkt.AddThree(DateTime.Now.ToEOTimeStamp());

			return m_client.SendPacket(pkt);
		}

		public bool DoCastSelfSpell(short spellID)
		{
			if (spellID < 0) return false;

			if (!Initialized || !m_client.ConnectedAndInitialized) return false;

			Packet pkt = new Packet(PacketFamily.Spell, PacketAction.TargetSelf);
			pkt.AddChar(1); //target type
			pkt.AddShort(spellID);
			pkt.AddInt(DateTime.Now.ToEOTimeStamp());

			return m_client.SendPacket(pkt);
		}

		public bool DoCastTargetSpell(short spellID, bool targetIsNPC, short targetID)
		{
			if (spellID < 0 || targetID < 0) return false;

			if (!Initialized || !m_client.ConnectedAndInitialized) return false;

			Packet pkt = new Packet(PacketFamily.Spell, PacketAction.TargetOther);
			pkt.AddChar((byte)(targetIsNPC ? 2 : 1));
			pkt.AddChar(1); //unknown value
			pkt.AddShort(1); //unknown value
			pkt.AddShort(spellID);
			pkt.AddShort(targetID);
			pkt.AddThree(DateTime.Now.ToEOTimeStamp());

			return m_client.SendPacket(pkt);
		}

		public bool DoCastGroupSpell(short spellID)
		{
			if (spellID < 0) return false;

			if (!Initialized || !m_client.ConnectedAndInitialized) return false;

			Packet pkt = new Packet(PacketFamily.Spell, PacketAction.TargetGroup);
			pkt.AddShort(spellID);
			pkt.AddThree(DateTime.Now.ToEOTimeStamp());

			return m_client.SendPacket(pkt);
		}

		#endregion

		#region handler methods

		private void _handleSpellRequest(Packet pkt)
		{
			short fromPlayerID = pkt.GetShort();
			short spellID = pkt.GetShort();

			if (OnOtherPlayerStartCastSpell != null)
				OnOtherPlayerStartCastSpell(fromPlayerID, spellID);
		}

		private void _handleSpellTargetSelf(Packet pkt)
		{
			short fromPlayerID = pkt.GetShort();
			short spellID = pkt.GetShort();
			int spellHP = pkt.GetInt();
			byte percentHealth = pkt.GetChar();

			if (pkt.ReadPos == pkt.Length)
			{
				//another player was the source of this packet

				if (OnOtherPlayerCastSpellSelf != null)
					OnOtherPlayerCastSpellSelf(fromPlayerID, spellID, spellHP, percentHealth);

				return;
			}

			short characterHP = pkt.GetShort();
			short characterTP = pkt.GetShort();
			if (pkt.GetShort() != 1) //malformed packet! eoserv sends '1' here
				return;

			//main player was source of this packet
			if (OnCastSpellSelf != null)
				OnCastSpellSelf(fromPlayerID, spellID, spellHP, percentHealth, characterHP, characterTP);
		}

		private void _handleSpellTargetOther(Packet pkt)
		{
			if (OnCastSpellTargetOther == null) return;

			short targetPlayerID = pkt.GetShort();
			short sourcePlayerID = pkt.GetShort();
			EODirection sourcePlayerDirection = (EODirection) pkt.GetChar();
			short spellID = pkt.GetShort();
			int recoveredHP = pkt.GetInt();
			byte targetPercentHealth = pkt.GetChar();

			short targetPlayerCurrentHP = -1;
			if (pkt.ReadPos != pkt.Length) //include current hp for player if main player is the target
				targetPlayerCurrentHP = pkt.GetShort();

			OnCastSpellTargetOther(
				targetPlayerID,
				sourcePlayerID,
				sourcePlayerDirection,
				spellID,
				recoveredHP,
				targetPercentHealth,
				targetPlayerCurrentHP);
		}

		private void _handleSpellTargetGroup(Packet pkt)
		{
			if (OnCastSpellTargetGroup == null) return;

			short spellID = pkt.GetShort();
			short fromPlayerID = pkt.GetShort();
			short fromPlayerTP = pkt.GetShort();
			short spellHealthGain = pkt.GetShort();

			var spellTargets = new List<GroupSpellTarget>();
			while (pkt.ReadPos != pkt.Length)
			{
				//malformed packet - eoserv puts 5 '255' bytes between party members
				if (pkt.GetBytes(5).Any(x => x != 255)) return;

				spellTargets.Add(new GroupSpellTarget(pkt));
			}

			OnCastSpellTargetGroup(spellID, fromPlayerID, fromPlayerTP, spellHealthGain, spellTargets);
		}

		#endregion
	}
}
