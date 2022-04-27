using Amadevus.RecordGenerator;
using EOLib.Domain.Spells;
using Optional;

namespace EOLib.Domain.NPC
{
    [Record]
    public sealed partial class NPC : ISpellTargetable
    {
        public int ID { get; }

        public int Index { get; }

        public byte X { get; }

        public byte Y { get; }

        public EODirection Direction { get; }

        public NPCFrame Frame { get; }

        public Option<short> OpponentID { get; }
    }
}