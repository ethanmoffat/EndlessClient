// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public event Action<string> OnStatusMessage;

        private void _createMessageMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Message, PacketAction.Open), _handleMessageOpen, true);
        }

        private void _handleMessageOpen(OldPacket pkt)
        {
            if (OnStatusMessage != null)
                OnStatusMessage(pkt.GetEndString());
        }
    }
}
