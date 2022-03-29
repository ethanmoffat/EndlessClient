using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public delegate void PlayerRecoverEvent(short HP, short TP);
    public delegate void RecoverReplyEvent(int exp, short karma, byte level, short statpoints = 0, short skillpoints = 0);
    public delegate void PlayerHealEvent(short playerID, int healAmount, byte percentHealth);

    partial class PacketAPI
    {
        public event RecoverReplyEvent OnRecoverReply;

        private void _createRecoverMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Recover, PacketAction.Reply), _handleRecoverReply, true);
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
    }
}
