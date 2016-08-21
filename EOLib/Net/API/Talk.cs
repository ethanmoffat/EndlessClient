// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Chat;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public delegate void ReceiveOtherChatEvent(ChatType type, string player, string message);
        public event ReceiveOtherChatEvent OnPlayerChatByName; //chat event that should only be shown in the chat panel
        public event Action<string> OnPMRecipientNotFound;
        public event Action<string> OnMuted;

        private void _createTalkMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Reply), _handleTalkReply, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Request), _handleTalkRequest, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Tell), _handleTalkTell, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Message), _handleTalkMessage, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Server), _handleTalkServer, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Admin), _handleTalkAdmin, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Announce), _handleTalkAnnounce, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Spec), _handleTalkSpec, true);
        }

        /// <summary>
        /// Handler for the TALK_REPLY packet (sent in response to not-found for PMs sent from this end)
        /// </summary>
        private void _handleTalkReply(OldPacket pkt)
        {
            if (OnPMRecipientNotFound == null) return;

            switch (pkt.GetShort())
            {
                //player is not found so a sys error needs to be displayed
                case 1: //TALK_NOTFOUND response (no other members of this enum)
                    string from = pkt.GetEndString();
                    from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
                    OnPMRecipientNotFound(from);
                    break;
            }
        }

        /// <summary>
        /// Handler for the TALK_TELL packet (sent in response to PM messages)
        /// </summary>
        private void _handleTalkTell(OldPacket pkt)
        {
            if (OnPlayerChatByName == null) return;

            string from = pkt.GetBreakString();
            from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
            string message = pkt.GetBreakString();
            OnPlayerChatByName(ChatType.PM, from, message);
        }

        /// <summary>
        /// Handler for the TALK_MESSAGE packet (sent in response to global messages)
        /// </summary>
        private void _handleTalkMessage(OldPacket pkt)
        {
            if (OnPlayerChatByName == null) return;

            string from = pkt.GetBreakString();
            from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
            string message = pkt.GetBreakString();

            OnPlayerChatByName(ChatType.Global, from, message);
        }

        /// <summary>
        /// Handler for the TALK_REQUEST packet (sent in response to guild messages)
        /// </summary>
        private void _handleTalkRequest(OldPacket pkt)
        {
            if (OnPlayerChatByName == null) return;

            string from = pkt.GetBreakString();
            from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
            string message = pkt.GetBreakString();

            OnPlayerChatByName(ChatType.Guild, from, message);
        }

        private void _handleTalkServer(OldPacket pkt)
        {
            if (OnPlayerChatByName == null) return;

            string msg = pkt.GetEndString();
            OnPlayerChatByName(ChatType.Server, null, msg);
        }

        private void _handleTalkAdmin(OldPacket pkt)
        {
            if (OnPlayerChatByName == null) return;

            string name = pkt.GetBreakString();
            name = char.ToUpper(name[0]) + name.Substring(1);
            string msg = pkt.GetBreakString();

            OnPlayerChatByName(ChatType.Admin, name, msg);
        }

        private void _handleTalkAnnounce(OldPacket pkt)
        {
            if (OnPlayerChatByName == null) return;

            string name = pkt.GetBreakString();
            name = char.ToUpper(name[0]) + name.Substring(1);
            string msg = pkt.GetBreakString();

            OnPlayerChatByName(ChatType.Announce, name, msg);
        }

        private void _handleTalkSpec(OldPacket pkt)
        {
            if (OnMuted != null)
                OnMuted(pkt.GetEndString());
        }
    }
}
