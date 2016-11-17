// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public delegate void PlayerAttackEvent(short playerID, EODirection dir);
        public event PlayerAttackEvent OnOtherPlayerAttack;

        private void _createAttackMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Attack, PacketAction.Player), _handleAttackPlayer, true);
        }

        /// <summary>
        /// Sent when another player attacks (not main player)
        /// </summary>
        private void _handleAttackPlayer(OldPacket pkt)
        {
            if (OnOtherPlayerAttack == null) return;
            short playerId = pkt.GetShort();
            EODirection dir = (EODirection)pkt.GetChar();
            OnOtherPlayerAttack(playerId, dir);
        }
    }
}
