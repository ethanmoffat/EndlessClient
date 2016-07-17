// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public struct ShopItem
    {
        private readonly int m_id, m_buy, m_sell, m_maxBuy;

        public int ID { get { return m_id; } }
        public int Buy { get { return m_buy; } }
        public int Sell { get { return m_sell; } }
        public int MaxBuy { get { return m_maxBuy; } }

        public ShopItem(int ID, int BuyPrice, int SellPrice, int MaxBuy)
        {
            m_id = ID;
            m_buy = BuyPrice;
            m_sell = SellPrice;
            m_maxBuy = MaxBuy;
        }
    }

    public struct CraftItem
    {
        private readonly int m_id;
        public int ID { get { return m_id; } }

        private readonly List<Tuple<int, int>> m_ingreds;
        public ReadOnlyCollection<Tuple<int, int>> Ingredients { get { return m_ingreds.AsReadOnly(); } }

        public CraftItem(int ID, IEnumerable<Tuple<int, int>> Ingredients)
        {
            m_ingreds = new List<Tuple<int, int>>();
            m_ingreds.AddRange(Ingredients.Where(x => x.Item1 != 0 && x.Item2 != 0));
            m_id = ID;
        }
    }

    partial class PacketAPI
    {
        public delegate void ShopOpenEvent(int shopID, string name, List<ShopItem> tradeItems, List<CraftItem> craftItems);
        public delegate void ShopTradeEvent(int goldRemaining, short itemID, int amount, byte weight, byte maxWeight, bool isBuy);
        public delegate void ShopCraftEvent(short itemID, byte weight, byte maxWeight, List<InventoryItem> ingredients);
        public event ShopOpenEvent OnShopOpen;
        public event ShopTradeEvent OnShopTradeItem;
        public event ShopCraftEvent OnShopCraftItem;

        private void _createShopMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Shop, PacketAction.Open), _handleShopOpen, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Shop, PacketAction.Buy), _handleShopBuy, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Shop, PacketAction.Sell), _handleShopSell, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Shop, PacketAction.Create), _handleShopCreate, true);
        }

        public bool RequestShop(short npcIndex)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Shop, PacketAction.Open);
            pkt.AddShort(npcIndex);

            return m_client.SendPacket(pkt);
        }

        /// <summary>
        /// Buy an item from a shopkeeper
        /// </summary>
        public bool BuyItem(short ItemID, int amount)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Shop, PacketAction.Buy);
            pkt.AddShort(ItemID);
            pkt.AddInt(amount);

            return m_client.SendPacket(pkt);
        }

        /// <summary>
        /// Sell an item to a shopkeeper
        /// </summary>
        public bool SellItem(short ItemID, int amount)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Shop, PacketAction.Sell);
            pkt.AddShort(ItemID);
            pkt.AddInt(amount);

            return m_client.SendPacket(pkt);
        }

        /// <summary>
        /// Craft an item with a shopkeeper
        /// </summary>
        public bool CraftItem(short ItemID)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Shop, PacketAction.Create);
            pkt.AddShort(ItemID);

            return m_client.SendPacket(pkt);
        }
        
        /// <summary>
        /// Handles SHOP_OPEN from server, contains shop data for a shop dialog
        /// </summary>
        private void _handleShopOpen(OldPacket pkt)
        {
            if (OnShopOpen == null) return;

            int shopKeeperID = pkt.GetShort();
            string shopName = pkt.GetBreakString();

            List<ShopItem> tradeItems = new List<ShopItem>();
            while (pkt.PeekByte() != 255)
            {
                ShopItem nextItem = new ShopItem(pkt.GetShort(), pkt.GetThree(), pkt.GetThree(), pkt.GetChar());
                tradeItems.Add(nextItem);
            }
            pkt.GetByte();

            List<CraftItem> craftItems = new List<CraftItem>();
            while (pkt.PeekByte() != 255)
            {
                int ID = pkt.GetShort();
                List<Tuple<int, int>> ingreds = new List<Tuple<int, int>>();

                for (int i = 0; i < 4; ++i)
                {
                    ingreds.Add(new Tuple<int, int>(pkt.GetShort(), pkt.GetChar()));
                }
                craftItems.Add(new CraftItem(ID, ingreds));
            }
            pkt.GetByte();

            OnShopOpen(shopKeeperID, shopName, tradeItems, craftItems);
        }

        /// <summary>
        /// Handles SHOP_BUY from server, response to buying an item
        /// </summary>
        private void _handleShopBuy(OldPacket pkt)
        {
            if (OnShopTradeItem == null) return;

            int charGoldLeft = pkt.GetInt();
            short itemID = pkt.GetShort();
            int amount = pkt.GetInt();
            byte weight = pkt.GetChar();
            byte maxWeight = pkt.GetChar();

            OnShopTradeItem(charGoldLeft, itemID, amount, weight, maxWeight, true);
        }

        /// <summary>
        /// Handles SHOP_SELL from server, response to selling an item
        /// </summary>
        private void _handleShopSell(OldPacket pkt)
        {
            if (OnShopTradeItem == null) return;

            int charNumLeft = pkt.GetInt();
            short itemID = pkt.GetShort();
            int charGold = pkt.GetInt();
            byte weight = pkt.GetChar();
            byte maxWeight = pkt.GetChar();

            OnShopTradeItem(charGold, itemID, charNumLeft, weight, maxWeight, false);
        }

        /// <summary>
        /// Handles SHOP_CREATE from server, response to crafting an item
        /// </summary>
        private void _handleShopCreate(OldPacket pkt)
        {
            if (OnShopCraftItem == null) return;

            short itemID = pkt.GetShort();
            byte weight = pkt.GetChar();
            byte maxWeight = pkt.GetChar();

            List<InventoryItem> inventoryItems = new List<InventoryItem>(4);
            while (pkt.ReadPos != pkt.Length)
            {
                if (pkt.PeekShort() <= 0) break;

                inventoryItems.Add(new InventoryItem(pkt.GetShort(), pkt.GetInt()));
            }

            OnShopCraftItem(itemID, weight, maxWeight, inventoryItems);
        }
    }
}
