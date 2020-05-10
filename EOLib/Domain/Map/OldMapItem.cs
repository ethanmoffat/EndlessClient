using System;

namespace EOLib.Domain.Map
{
    //todo: remove use of OldMapItem for IMapItem
    public class OldMapItem
    {
        public short UniqueID { get; set; }
        public short ItemID { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public int Amount { get; set; }
        public DateTime DropTime { get; set; }
        public bool IsNPCDrop { get; set; }
        public int OwningPlayerID { get; set; }
    }
}