using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public struct ChestData
    {
        private readonly List<InventoryItem> _items;

        public byte X { get; private set; }
        public byte Y { get; private set; }

        public IList<InventoryItem> Items
        {
            get
            {
                var itemsToReturn = _items.Select(x => new InventoryItem(x.ItemID, x.Amount));
                return itemsToReturn.ToList();
            }
        }

        internal ChestData(OldPacket pkt, bool containsCoords)
            : this()
        {
            X = containsCoords ? pkt.GetChar() : byte.MinValue;
            Y = containsCoords ? pkt.GetChar() : byte.MinValue;

            var numRemaining = pkt.PeekEndString().Length / 5;
            _items = new List<InventoryItem>(numRemaining);
            for (var i = 0; i < numRemaining; ++i)
            {
                _items.Add(new InventoryItem(pkt.GetShort(), pkt.GetThree()));
            }
        }
    }

    partial class PacketAPI
    {
        public delegate void ChestDataChangeEvent(short id, int amount, byte weight, byte maxWeight, ChestData data);

        public event Action<ChestData> OnChestOpened;
        public event Action<ChestData> OnChestAgree;
        public event ChestDataChangeEvent OnChestGetItem;
        public event ChestDataChangeEvent OnChestAddItem;

        private void _createChestMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Chest, PacketAction.Open), _handleChestOpen, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Chest, PacketAction.Get), _handleChestGet, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Chest, PacketAction.Agree), _handleChestAgree, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Chest, PacketAction.Reply), _handleChestReply, true);
        }

        public bool ChestOpen(byte x, byte y)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket toSend = new OldPacket(PacketFamily.Chest, PacketAction.Open);
            toSend.AddChar(x);
            toSend.AddChar(y);

            return m_client.SendPacket(toSend);
        }

        public bool ChestTakeItem(byte x, byte y, short itemID)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Chest, PacketAction.Take);
            pkt.AddChar(x);
            pkt.AddChar(y);
            pkt.AddShort(itemID);

            return m_client.SendPacket(pkt);
        }

        public bool ChestAddItem(byte x, byte y, short itemID, int amount)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Chest, PacketAction.Add);
            pkt.AddChar(x);
            pkt.AddChar(y);
            pkt.AddShort(itemID);
            pkt.AddThree(amount);

            return m_client.SendPacket(pkt);
        }

        /// <summary>
        /// Handler for CHEST_OPEN packet, sent in response to main player opening a chest
        /// </summary>
        private void _handleChestOpen(OldPacket pkt)
        {
            if (OnChestOpened != null)
                OnChestOpened(new ChestData(pkt, true));
        }

        /// <summary>
        /// Handler for CHEST_GET packet, sent as confirmation to character that item is being taken
        /// </summary>
        private void _handleChestGet(OldPacket pkt)
        {
            if (OnChestGetItem == null) return;
            short takenID = pkt.GetShort();
            int takenAmount = pkt.GetThree();
            byte characterWeight = pkt.GetChar();
            byte characterMaxWeight = pkt.GetChar();
            ChestData data = new ChestData(pkt, false);
            OnChestGetItem(takenID, takenAmount, characterWeight, characterMaxWeight, data);
        }

        /// <summary>
        /// Handler for CHEST_AGREE packet, sent as update to other characters near a chest that was modified by another player
        /// </summary>
        private void _handleChestAgree(OldPacket pkt)
        {
            if (OnChestAgree != null)
                OnChestAgree(new ChestData(pkt, false));
        }

        /// <summary>
        /// Handler for CHEST_REPLY packet, sent in response to main player adding an item to a chest
        /// </summary>
        private void _handleChestReply(OldPacket pkt)
        {
            if (OnChestAddItem == null) return;

            short remainingID = pkt.GetShort();
            int remainingAmount = pkt.GetInt();
            byte characterWeight = pkt.GetChar();
            byte characterMaxWeight = pkt.GetChar();
            ChestData data = new ChestData(pkt, false);
            OnChestAddItem(remainingID, remainingAmount, characterWeight, characterMaxWeight, data);
        }
    }
}
