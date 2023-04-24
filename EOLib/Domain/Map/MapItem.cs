using Amadevus.RecordGenerator;
using Optional;
using System;

namespace EOLib.Domain.Map
{
    [Record]
    public sealed partial class MapItem
    {
        public int UniqueID { get; }

        public int ItemID { get; }

        public int X { get; }

        public int Y { get; }

        public int Amount { get; }

        public bool IsNPCDrop { get; }

        public Option<int> OwningPlayerID { get; }

        public Option<DateTime> DropTime { get; }

        public static MapItem None => new MapItem();

        public MapItem(int uid, int id, int x, int y, int amount)
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