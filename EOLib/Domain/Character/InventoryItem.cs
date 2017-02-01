// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Character
{
    public class InventoryItem : IInventoryItem
    {
        public short ItemID { get; }
        
        public int Amount { get; }

        public InventoryItem(short itemID, int amount)
        {
            ItemID = itemID;
            Amount = amount;
        }

        public IInventoryItem WithAmount(int newAmount)
        {
            return new InventoryItem(ItemID, newAmount);
        }
    }

    public interface IInventoryItem
    {
        short ItemID { get; }

        int Amount { get; }

        IInventoryItem WithAmount(int newAmount);
    }
}