using Amadevus.RecordGenerator;

namespace EOLib.Domain.Character
{
    [Record]
    public sealed partial class InventorySpell
    {
        public int ID { get; }

        public int Level { get; }
    }
}