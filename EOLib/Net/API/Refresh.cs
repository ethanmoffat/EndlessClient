// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        private void _createRefreshMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Refresh, PacketAction.Reply), _handleRefreshReply, true);
        }

        /// <summary>
        /// Refreshes the client view - response will include other Characters, NPCs, and Map Items (see OnWarpAgree)
        /// </summary>
        /// <returns>True on successful send operation, false on failure</returns>
        public bool RequestRefresh()
        {
            //no data sent to server - just expecting a reply
            return m_client.ConnectedAndInitialized && Initialized &&
                   m_client.SendPacket(new OldPacket(PacketFamily.Refresh, PacketAction.Request));
        }

        private void _handleRefreshReply(OldPacket pkt)
        {
            //if (OnWarpAgree == null)
            //    return;

            //byte numOtherChars = pkt.GetChar();
            //if (pkt.GetByte() != 255)
            //    return;

            //List<CharacterData> otherChars = new List<CharacterData>(numOtherChars);
            //for (int i = 0; i < numOtherChars; ++i)
            //{
            //    CharacterData data = new CharacterData(pkt);
            //    otherChars.Add(data);
            //    if (pkt.GetByte() != 255)
            //        return;
            //}

            //List<NPCData> otherNPCs = new List<NPCData>();
            //while (pkt.PeekByte() != 255)
            //{
            //    NPCData newGuy = new NPCData(pkt);
            //    otherNPCs.Add(newGuy);
            //}
            //pkt.GetByte();

            //List<OldMapItem> mapItems = new List<OldMapItem>();
            //while (pkt.ReadPos < pkt.Length)
            //{
            //    mapItems.Add(new OldMapItem
            //    {
            //        UniqueID = pkt.GetShort(),
            //        ItemID = pkt.GetShort(),
            //        X = pkt.GetChar(),
            //        Y = pkt.GetChar(),
            //        Amount = pkt.GetThree(),
            //        //turn off drop protection for items coming into view - server will validate
            //        DropTime = DateTime.Now.AddSeconds(-5),
            //        IsNPCDrop = false,
            //        OwningPlayerID = -1
            //    });
            //}

            //OnWarpAgree(-1, WarpAnimation.None, otherChars, otherNPCs, mapItems);
        }
    }
}
