using Amadevus.RecordGenerator;

namespace EOLib.Domain.Character
{
    [Record]
    public sealed partial class InventorySpell
    {
        public short ID { get; }

        public short Level { get; }
    }
}