// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public delegate void PlayerRecoverEvent(short HP, short TP);
    public delegate void RecoverReplyEvent(int exp, short karma, byte level, short statpoints = 0, short skillpoints = 0);
    public delegate void PlayerHealEvent(short playerID, int healAmount, byte percentHealth);

    partial class PacketAPI
    {
        public event RecoverReplyEvent OnRecoverReply;
        public event PlayerHealEvent OnPlayerHeal;

        private void _createRecoverMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Recover, PacketAction.Reply), _handleRecoverReply, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Recover, PacketAction.Agree), _handleRecoverAgree, true);
        }

        private void _handleRecoverReply(OldPacket pkt)
        {
            if (OnRecoverReply == null) return;

            int exp = pkt.GetInt();
            short karma = pkt.GetShort();
            byte level = pkt.GetChar();

            if (pkt.ReadPos == pkt.Length)
                OnRecoverReply(exp, karma, level);
            else
            {
                short statpoints = pkt.GetShort();
                short skillpoints = pkt.GetShort();

                OnRecoverReply(exp, karma, level, statpoints, skillpoints);
            }
        }

        private void _handleRecoverAgree(OldPacket pkt)
        {
            //when a heal item is used by another player
            if (OnPlayerHeal != null)
                OnPlayerHeal(pkt.GetShort(), pkt.GetInt(), pkt.GetChar()); //player id - hp gain - percent heal
        }
    }
}
