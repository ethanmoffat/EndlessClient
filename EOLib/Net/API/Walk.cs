// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public delegate void AddMapItemsEvent(List<OldMapItem> items);
        public delegate void OtherPlayerWalkEvent(short id, EODirection dir, byte x, byte y);

        public event AddMapItemsEvent OnMainPlayerWalk;

        private void _createWalkMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Walk, PacketAction.Reply), _handleMainPlayerWalk, true);
        }

        private void _handleMainPlayerWalk(OldPacket pkt)
        {
            if (pkt.GetByte() != 255 || pkt.GetByte() != 255 || OnMainPlayerWalk == null)
                return;

            //response contains the map items that are now in range
            int numberOfMapItems = pkt.PeekEndString().Length / 9;
            List<OldMapItem> items = new List<OldMapItem>(numberOfMapItems);
            for (int i = 0; i < numberOfMapItems; ++i)
            {
                items.Add(new OldMapItem
                {
                    UniqueID = pkt.GetShort(),
                    ItemID = pkt.GetShort(),
                    X = pkt.GetChar(),
                    Y = pkt.GetChar(),
                    Amount = pkt.GetThree()
                });
            }

            OnMainPlayerWalk(items);
        }
    }
}
