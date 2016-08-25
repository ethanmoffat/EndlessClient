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
        public event Action<string> OnMuted;

        private void _createTalkMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Talk, PacketAction.Spec), _handleTalkSpec, true);
        }

        private void _handleTalkSpec(OldPacket pkt)
        {
            if (OnMuted != null)
                OnMuted(pkt.GetEndString());
        }
    }
}
