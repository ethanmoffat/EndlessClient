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
