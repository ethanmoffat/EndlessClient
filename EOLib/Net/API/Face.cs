// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public event Action<short, EODirection> OnPlayerFace;

        private void _createFaceMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Face, PacketAction.Player), _handleFacePlayer, true);
        }

        //todo: this can be removed
        public bool FacePlayer(EODirection dir)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Face, PacketAction.Player);
            pkt.AddChar((byte)dir);

            return m_client.SendPacket(pkt);
        }

        private void _handleFacePlayer(OldPacket pkt)
        {
            short playerId = pkt.GetShort();
            EODirection dir = (EODirection)pkt.GetChar();

            if (OnPlayerFace != null)
                OnPlayerFace(playerId, dir);
        }
    }
}
