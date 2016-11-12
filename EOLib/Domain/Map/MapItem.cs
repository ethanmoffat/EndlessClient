// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Map
{
    public class MapItem : IMapItem
    {
        public short UniqueID { get; private set; }

        public short ItemID { get; private set; }

        public byte X { get; private set; }

        public byte Y { get; private set; }

        public int Amount { get; private set; }

        public MapItem(short uid, short itemID, byte x, byte y)
        {
            UniqueID = uid;
            ItemID = itemID;
            X = x;
            Y = y;
        }

        public IMapItem WithAmount(int newAmount)
        {
            return new MapItem(UniqueID, ItemID, X, Y) { Amount = newAmount };
        }
    }

    public interface IMapItem
    {
        short UniqueID { get; }

        short ItemID { get; }

        byte X { get; }

        byte Y { get; }

        int Amount { get; }

        IMapItem WithAmount(int newAmount);
    }
}