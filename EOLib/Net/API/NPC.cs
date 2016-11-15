// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public delegate void NPCLeaveMapEvent(byte index, int damageToNPC, short playerID, EODirection playerDirection, short tpRemaining = -1, short spellID = -1);
    public delegate void NPCKilledEvent(int exp);
    public delegate void NPCTakeDamageEvent(byte npcIndex, short fromPlayerID, EODirection fromDirection, int damageToNPC, int npcPctHealth, short spellID = -1, short fromTP = -1);

    partial class PacketAPI
    {
        public event NPCLeaveMapEvent OnNPCLeaveMap;
        public event NPCKilledEvent OnNPCKilled; //int is the experience gained
        public event NPCTakeDamageEvent OnNPCTakeDamage;
        public event Action<LevelUpStats> OnPlayerLevelUp;
        public event Action<short> OnRemoveChildNPCs;

        private void _createNPCMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Accept), _handleNPCAccept, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Cast, PacketAction.Accept), _handleNPCAccept, true);

            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Reply), _handleNPCReply, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Cast, PacketAction.Reply), _handleNPCReply, true);

            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Spec), _handleNPCSpec, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Cast, PacketAction.Spec), _handleNPCSpec, true);

            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Junk), _handleNPCJunk, true);
        }

        /// <summary>
        /// Handler for NPC_SPEC packet, when NPC should be removed from view - either by dying or out of character range
        /// </summary>
        private void _handleNPCSpec(OldPacket pkt)
        {
            short spellID = -1;
            if (pkt.Family == PacketFamily.Cast)
                spellID = pkt.GetShort();

            short playerID = pkt.GetShort(); //player that is protecting the item
            EODirection playerDirection = (EODirection)pkt.GetChar();
            short deadNPCIndex = pkt.GetShort();

            if (pkt.ReadPos == pkt.Length)
            {
                if (OnNPCLeaveMap != null)
                    OnNPCLeaveMap((byte) deadNPCIndex, 0, playerID, playerDirection);
                return; //just removing from range, packet ends here
            }

            short droppedItemUID = pkt.GetShort();
            short droppedItemID = pkt.GetShort();
            byte x = pkt.GetChar();
            byte y = pkt.GetChar();
            int droppedAmount = pkt.GetInt();
            int damageDoneToNPC = pkt.GetThree();

            short characterTPRemaining = -1;
            if (pkt.Family == PacketFamily.Cast)
                characterTPRemaining = pkt.GetShort();

            if(OnNPCLeaveMap != null)
                OnNPCLeaveMap((byte)deadNPCIndex, damageDoneToNPC, playerID, playerDirection, characterTPRemaining, spellID);

            //just showing a dropped item, packet ends here
            if (pkt.ReadPos == pkt.Length)
            {
                _showDroppedItemIfNeeded(playerID, droppedItemID, droppedAmount, droppedItemUID, x, y);
                return;
            }

            int newExp = pkt.GetInt(); //npc was killed - this handler was invoked from NPCAccept
            if (OnNPCKilled != null)
                OnNPCKilled(newExp);

            //the order in the original client is: 'you gained {x} EXP' and then 'The NPC dropped {x}'
            //Otherwise, I would just do the drop item logic once above
            _showDroppedItemIfNeeded(playerID, droppedItemID, droppedAmount, droppedItemUID, x, y);
        }

        private void _showDroppedItemIfNeeded(short playerID, short droppedItemID, int droppedAmount, short droppedItemUID, byte x, byte y)
        {
            if (droppedItemID > 0 && OnDropItem != null)
            {
                OnDropItem(-1, 0, 0, new OldMapItem
                {
                    Amount = droppedAmount,
                    ItemID = droppedItemID,
                    UniqueID = droppedItemUID,
                    X = x,
                    Y = y,
                    DropTime = DateTime.Now,
                    IsNPCDrop = true,
                    OwningPlayerID = playerID
                });
            }
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

        /// <summary>
        /// Handler for NPC_ACCEPT packet, when character levels up from exp earned when killing an NPC
        /// </summary>
        private void _handleNPCAccept(OldPacket pkt)
        {
            _handleNPCSpec(pkt); //same handler for the first part of the packet

            if (OnPlayerLevelUp != null)
                OnPlayerLevelUp(new LevelUpStats(pkt, false));
        }

        private void _handleNPCJunk(OldPacket pkt)
        {
            if (OnRemoveChildNPCs != null)
                OnRemoveChildNPCs(pkt.GetShort());
        }
    }
}
