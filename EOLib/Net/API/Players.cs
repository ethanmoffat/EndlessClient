// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public delegate void PlayerEnterMapEvent(CharacterData data, WarpAnimation anim = WarpAnimation.None);

        public event PlayerEnterMapEvent OnPlayerEnterMap;

        private void _createPlayersMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Players, PacketAction.Agree), _handlePlayersAgree, true);
        }

        // Handles PLAYERS_AGREE packet which is sent when a player enters a map by warp or upon spawning
        private void _handlePlayersAgree(OldPacket pkt)
        {
            if (pkt.GetByte() != 255 || OnPlayerEnterMap == null)
                return;

            CharacterData newGuy = new CharacterData(pkt);

            byte nextByte = pkt.GetByte();
            WarpAnimation anim = WarpAnimation.None;
            if (nextByte != 255) //next byte was the warp animation: sent on Map::Enter in eoserv
            {
                pkt.Skip(-1);
                anim = (WarpAnimation)pkt.GetChar();
                if (pkt.GetByte() != 255) //the 255 still needs to be read...
                    return;
            }
            //else... //next byte was a 255. proceed normally.

            if (pkt.GetChar() == 1) //0 for NPC, 1 for player. In eoserv it is never 0.
                OnPlayerEnterMap(newGuy, anim);
        }
    }
}
