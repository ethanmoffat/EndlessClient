using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public struct GroupSpellTarget
    {
        private readonly short _partyMemberID, _partyMemberPercentHealth, _partyMemberHP;

        public short MemberID => _partyMemberID;
        public short MemberPercentHealth => _partyMemberPercentHealth;
        public short MemberHP => _partyMemberHP;

        internal GroupSpellTarget(OldPacket pkt)
        {
            _partyMemberID = pkt.GetShort();
            _partyMemberPercentHealth = pkt.GetChar();
            _partyMemberHP = pkt.GetShort();
        }
    }

    #region event delegates

    public delegate void CastSpellGroupEvent(short spellID, short fromPlayerID, short fromPlayerTP, short spellHPGain, List<GroupSpellTarget> spellTargets);

    #endregion

    partial class PacketAPI
    {
        #region public events

        public event CastSpellGroupEvent OnCastSpellTargetGroup;

        #endregion

        #region initialization

        private void _createSpellMembers()
        {
            //note: see CAST_REPLY handler in _handleNPCReply for NPCs taking damage from a spell. handler is almost identical
            //        see CAST_ACCPT handler for leveling up off NPC kill via spell
            //        see CAST_SPEC  handler for regular NPC death via spell
            //other note: Spell attacks for PK are not supported yet
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Spell, PacketAction.TargetGroup), _handleSpellTargetGroup, true);
        }

        #endregion

        #region public API

        public bool PrepareCastSpell(short spellID)
        {
            if (spellID < 0) return false; //integer overflow resulted in negative number - server expects ushort

            if (!Initialized || !m_client.ConnectedAndInitialized) return false;

            OldPacket pkt = new OldPacket(PacketFamily.Spell, PacketAction.Request);
            pkt.AddShort(spellID);
            pkt.AddThree(DateTime.Now.ToEOTimeStamp());

            return m_client.SendPacket(pkt);
        }

        public bool DoCastSelfSpell(short spellID)
        {
            if (spellID < 0) return false;

            if (!Initialized || !m_client.ConnectedAndInitialized) return false;

            OldPacket pkt = new OldPacket(PacketFamily.Spell, PacketAction.TargetSelf);
            pkt.AddChar(1); //target type
            pkt.AddShort(spellID);
            pkt.AddInt(DateTime.Now.ToEOTimeStamp());

            return m_client.SendPacket(pkt);
        }

        public bool DoCastTargetSpell(short spellID, bool targetIsNPC, short targetID)
        {
            if (spellID < 0 || targetID < 0) return false;

            if (!Initialized || !m_client.ConnectedAndInitialized) return false;

            OldPacket pkt = new OldPacket(PacketFamily.Spell, PacketAction.TargetOther);
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

            OldPacket pkt = new OldPacket(PacketFamily.Spell, PacketAction.TargetGroup);
            pkt.AddShort(spellID);
            pkt.AddThree(DateTime.Now.ToEOTimeStamp());

            return m_client.SendPacket(pkt);
        }

        #endregion

        #region handler methods

        private void _handleSpellTargetGroup(OldPacket pkt)
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
