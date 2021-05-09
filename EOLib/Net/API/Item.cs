using System;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public delegate void PlayerItemDropEvent(int characterAmount, byte weight, byte maxWeight, OldMapItem item);
    public delegate void RemoveMapItemEvent(short itemUID);
    public delegate void JunkItemEvent(short itemID, int numJunked, int numRemaining, byte weight, byte maxWeight);
    public delegate void ItemChangeEvent(bool newItemObtained, short id, int amount, byte weight);

    partial class PacketAPI
    {
        /// <summary>
        /// Occurs when any player drops an item - if characterAmount == -1, this means the item was dropped by a player other than MainPlayer
        /// </summary>
        public event PlayerItemDropEvent OnDropItem;
        public event RemoveMapItemEvent OnRemoveItemFromMap;
        public event JunkItemEvent OnJunkItem;
        public event ItemChangeEvent OnItemChange;

        private void _createItemMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Drop), _handleItemDrop, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Add), _handleItemAdd, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Remove), _handleItemRemove, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Junk), _handleItemJunk, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Obtain), _handleItemObtain, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Kick), _handleItemKick, true);
            //todo: handle ITEM_ACCEPT (ExpReward item type) (I think it shows the level up animation?)
        }

        /// <summary>
        /// Pick up the item with the specified UID
        /// </summary>
        public bool GetItem(short uid)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Item, PacketAction.Get);
            pkt.AddShort(uid);

            return m_client.SendPacket(pkt);
        }

        public bool DropItem(short id, int amount, byte x = 255, byte y = 255) //255 means use character's current location
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Item, PacketAction.Drop);
            pkt.AddShort(id);
            pkt.AddInt(amount);
            if (x == 255 && y == 255)
            {
                pkt.AddByte(x);
                pkt.AddByte(y);
            }
            else
            {
                pkt.AddChar(x);
                pkt.AddChar(y);
            }

            return m_client.SendPacket(pkt);
        }

        public bool JunkItem(short id, int amount)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Item, PacketAction.Junk);
            pkt.AddShort(id);
            pkt.AddInt(amount);

            return m_client.SendPacket(pkt);
        }
        
        private void _handleItemDrop(OldPacket pkt)
        {
            if (OnDropItem == null) return;
            short _id = pkt.GetShort();
            int _amount = pkt.GetThree();
            int characterAmount = pkt.GetInt(); //amount remaining for the character
            OldMapItem item = new OldMapItem
            {
                ItemID = _id,
                Amount = _amount,
                UniqueID = pkt.GetShort(),
                X = pkt.GetChar(),
                Y = pkt.GetChar(),
                //turn off drop protection since main player dropped it
                DropTime = DateTime.Now.AddSeconds(-5),
                IsNPCDrop = false,
                OwningPlayerID = 0 //id of 0 means the currently logged in player owns it
            };
            byte characterWeight = pkt.GetChar(), characterMaxWeight = pkt.GetChar(); //character adjusted weights
            
            OnDropItem(characterAmount, characterWeight, characterMaxWeight, item);
        }

        private void _handleItemAdd(OldPacket pkt)
        {
            if (OnDropItem == null) return;
            OldMapItem item = new OldMapItem
            {
                ItemID = pkt.GetShort(),
                UniqueID = pkt.GetShort(),
                Amount = pkt.GetThree(),
                X = pkt.GetChar(),
                Y = pkt.GetChar(),
                DropTime = DateTime.Now,
                IsNPCDrop = false,
                OwningPlayerID = -1 //another player dropped. drop protection says "Item protected" w/o player name
            };
            OnDropItem(-1, 0, 0, item);
        }

        private void _handleItemRemove(OldPacket pkt)
        {
            if (OnRemoveItemFromMap != null)
                OnRemoveItemFromMap(pkt.GetShort());
        }

        private void _handleItemJunk(OldPacket pkt)
        {
            short id = pkt.GetShort();
            int amountRemoved = pkt.GetThree();//don't really care - just math it
            int amountRemaining = pkt.GetInt();
            byte weight = pkt.GetChar();
            byte maxWeight = pkt.GetChar();

            if (OnJunkItem != null)
                OnJunkItem(id, amountRemoved, amountRemaining, weight, maxWeight);
        }

        private void _handleItemObtain(OldPacket pkt)
        {
            if (OnItemChange == null) return;

            short id = pkt.GetShort();
            int amount = pkt.GetThree();
            byte weight = pkt.GetChar();

            OnItemChange(true, id, amount, weight);
        }

        private void _handleItemKick(OldPacket pkt)
        {
            if (OnItemChange == null) return;

            short id = pkt.GetShort();
            int amount = pkt.GetThree();
            byte weight = pkt.GetChar();

            OnItemChange(false, id, amount, weight);
        }
    }
}
