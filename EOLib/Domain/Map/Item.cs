// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Map
{
    public class Item : IItem
    {
        public short UniqueID { get; private set; }

        public short ItemID { get; private set; }

        public byte X { get; private set; }

        public byte Y { get; private set; }

        public int Amount { get; private set; }

        public Item(short uid, short itemID, byte x, byte y)
        {
            UniqueID = uid;
            ItemID = itemID;
            X = x;
            Y = y;
        }

        public IItem WithAmount(int newAmount)
        {
            return new Item(UniqueID, ItemID, X, Y) { Amount = newAmount };
        }
    }

    public interface IItem
    {
        short UniqueID { get; }

        short ItemID { get; }

        byte X { get; }

        byte Y { get; }

        int Amount { get; }

        IItem WithAmount(int newAmount);
    }
}