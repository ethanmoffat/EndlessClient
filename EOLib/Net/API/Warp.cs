// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public enum WarpAnimation
    {
        None,
        Scroll,
        Admin,
        Invalid = 255
    }

    partial class PacketAPI
    {
        /// <summary>
        /// Defines a delegate for a warp agree event (when WARP_AGREE is received from server)
        /// </summary>
        public delegate void WarpAgreeEvent(short MapID, WarpAnimation anim, List<CharacterData> charData, List<NPCData> npcData, List<OldMapItem> itemData);

        public event WarpAgreeEvent OnWarpAgree;

        private void _createWarpMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Warp, PacketAction.Agree), _handleWarpAgree, true);
        }

        private void _handleWarpAgree(OldPacket pkt)
        {
            if (pkt.GetChar() != 2 || OnWarpAgree == null) return;

            short mapID = pkt.GetShort();
            WarpAnimation anim = (WarpAnimation)pkt.GetChar();

            int numOtherCharacters = pkt.GetChar();
            if (pkt.GetByte() != 255) return;

            List<CharacterData> otherCharacters = new List<CharacterData>(numOtherCharacters - 1);
            for (int i = 0; i < numOtherCharacters; ++i)
            {
                CharacterData newChar = new CharacterData(pkt);
                otherCharacters.Add(newChar);
                if (pkt.GetByte() != 255) return;
            }

            List<NPCData> otherNPCs = new List<NPCData>();
            while (pkt.PeekByte() != 255)
            {
                otherNPCs.Add(new NPCData(pkt));
            }
            if (pkt.GetByte() != 255) return;

            List<OldMapItem> otherItems = new List<OldMapItem>();
            while (pkt.ReadPos < pkt.Length)
            {
                otherItems.Add(new OldMapItem
                {
                    UniqueID = pkt.GetShort(),
                    ItemID = pkt.GetShort(),
                    X = pkt.GetChar(),
                    Y = pkt.GetChar(),
                    Amount = pkt.GetThree(),
                    //turn off drop protection for items coming into view - server will validate
                    DropTime = DateTime.Now.AddSeconds(-5),
                    IsNPCDrop = false,
                    OwningPlayerID = -1
                });
            }

            OnWarpAgree(mapID, anim, otherCharacters, otherNPCs, otherItems);
        }
    }
}
