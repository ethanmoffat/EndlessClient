// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public delegate void NPCTakeDamageEvent(byte npcIndex, short fromPlayerID, EODirection fromDirection, int damageToNPC, int npcPctHealth, short spellID = -1, short fromTP = -1);

    partial class PacketAPI
    {
        public event NPCTakeDamageEvent OnNPCTakeDamage;
        public event Action<short> OnRemoveChildNPCs;

        private void _createNPCMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Reply), _handleNPCReply, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Cast, PacketAction.Reply), _handleNPCReply, true);

            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Junk), _handleNPCJunk, true);
        }

        /// <summary>
        /// Handler for NPC_REPLY packet, when NPC takes damage from an attack (spell cast or weapon) but is still alive
        /// </summary>
        private void _handleNPCReply(OldPacket pkt)
        {
            if (OnNPCTakeDamage == null) return;

            short spellID = -1;
            if (pkt.Family == PacketFamily.Cast)
                spellID = pkt.GetShort();

            short fromPlayerID = pkt.GetShort();
            EODirection fromDirection = (EODirection)pkt.GetChar();
            short npcIndex = pkt.GetShort();
            int damageToNPC = pkt.GetThree();
            int npcPctHealth = pkt.GetShort();

            short fromTP = -1;
            if (pkt.Family == PacketFamily.Cast)
                fromTP = pkt.GetShort();
            else if (pkt.GetChar() != 1) //some constant 1 in EOSERV
                return;

            OnNPCTakeDamage((byte)npcIndex, fromPlayerID, fromDirection, damageToNPC, npcPctHealth, spellID, fromTP);
        }

        private void _handleNPCJunk(OldPacket pkt)
        {
            if (OnRemoveChildNPCs != null)
                OnRemoveChildNPCs(pkt.GetShort());
        }
    }
}
