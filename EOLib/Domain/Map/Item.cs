// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Domain.Map
{
    public class Item : IItem
    {
        public short UniqueID { get; }

        public short ItemID { get; }

        public byte X { get; }

        public byte Y { get; }

        public int Amount { get; private set; }

        public bool IsNPCDrop { get; private set; }

        public Optional<int> OwningPlayerID { get; private set; }

        public Optional<DateTime> DropTime { get; private set; }

        public Item(short uid, short itemID, byte x, byte y)
        {
            UniqueID = uid;
            ItemID = itemID;
            X = x;
            Y = y;
        }

        public IItem WithAmount(int newAmount)
        {
            var newItem = MakeCopy(this);
            newItem.Amount = newAmount;
            return newItem;
        }

        public IItem WithIsNPCDrop(bool isNPCDrop)
        {
            var newItem = MakeCopy(this);
            newItem.IsNPCDrop = isNPCDrop;
            return newItem;
        }

        public IItem WithOwningPlayerID(Optional<int> owningPlayerID)
        {
            var newItem = MakeCopy(this);
            newItem.OwningPlayerID = owningPlayerID;
            return newItem;
        }

        public IItem WithDropTime(Optional<DateTime> dropTime)
        {
            var newItem = MakeCopy(this);
            newItem.DropTime = dropTime;
            return newItem;
        }

        private static Item MakeCopy(IItem input)
        {
            return new Item(input.UniqueID, input.ItemID, input.X, input.Y)
            {
                Amount = input.Amount,
                IsNPCDrop = input.IsNPCDrop,
                OwningPlayerID = input.OwningPlayerID,
                DropTime = input.DropTime
            };
        }
    }

    public interface IItem
    {
        short UniqueID { get; }

        short ItemID { get; }

        byte X { get; }

        byte Y { get; }

        int Amount { get; }

        bool IsNPCDrop { get; }

        Optional<int> OwningPlayerID { get; }

        Optional<DateTime> DropTime { get; }

        IItem WithAmount(int newAmount);

        IItem WithIsNPCDrop(bool isNPCDrop);

        IItem WithOwningPlayerID(Optional<int> owningPlayerID);

        IItem WithDropTime(Optional<DateTime> dropTime);
    }
}