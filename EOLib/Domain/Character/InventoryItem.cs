using Amadevus.RecordGenerator;

namespace EOLib.Domain.Character
{
    [Record]
    public sealed partial class InventoryItem
    {
        public short ItemID { get; }
        
        public int Amount { get; }
    }
}