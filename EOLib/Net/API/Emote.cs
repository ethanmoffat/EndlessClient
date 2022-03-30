using System;
using EOLib.Domain.Character;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public event Action<short, Emote> OnOtherPlayerEmote;

        private void _createEmoteMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Emote, PacketAction.Player), _handleEmotePlayer, true);
        }

        public bool ReportEmote(Emote emote)
        {
            //trade/level up happen differently
            if (emote == Emote.Trade || emote == Emote.LevelUp)
                return false; //signal error client-side

            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Emote, PacketAction.Report);
            pkt.AddChar((byte)emote);

            return m_client.SendPacket(pkt);
        }

        private void _handleEmotePlayer(OldPacket pkt)
        {
            short playerID = pkt.GetShort();
            Emote emote = (Emote)pkt.GetChar();

            if(OnOtherPlayerEmote != null)
                OnOtherPlayerEmote(playerID, emote);
        }
    }
}
