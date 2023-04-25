using Amadevus.RecordGenerator;

namespace EOLib.Domain.Character
{
    [Record(Features.Default | Features.EquatableEquals | Features.ObjectEquals)]
    public sealed partial class InventoryItem
    {
        public int ItemID { get; }
        
        public int Amount { get; }
    }
}