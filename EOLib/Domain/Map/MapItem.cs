using Amadevus.RecordGenerator;
using Optional;
using System;

namespace EOLib.Domain.Map
{
    [Record]
    public sealed partial class MapItem
    {
        public short UniqueID { get; }

        public short ItemID { get; }

        public byte X { get; }

        public byte Y { get; }

        public int Amount { get; }

        public bool IsNPCDrop { get; }

        public Option<int> OwningPlayerID { get; }

        public Option<DateTime> DropTime { get; }

        public static MapItem None => new MapItem();

        public MapItem(short uid, short id, byte x, byte y, int amount)
        {
            UniqueID = uid;
            ItemID = id;
            X = x;
            Y = y;
            Amount = amount;
        }

        private MapItem() { }
    }
}