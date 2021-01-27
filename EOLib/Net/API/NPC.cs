using System;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public event Action<short> OnRemoveChildNPCs;

        private void _createNPCMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Junk), _handleNPCJunk, true);
        }

        private void _handleNPCJunk(OldPacket pkt)
        {
            if (OnRemoveChildNPCs != null)
                OnRemoveChildNPCs(pkt.GetShort());
        }
    }
}
