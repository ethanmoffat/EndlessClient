using System;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public event Action<int> OnPlaySoundEffect;

        private void _createMusicMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Music, PacketAction.Player), _handleMusicPlayer, true);
        }

        private void _handleMusicPlayer(OldPacket pkt)
        {
            if (OnPlaySoundEffect != null)
                OnPlaySoundEffect(pkt.GetChar());
        }
    }
}
