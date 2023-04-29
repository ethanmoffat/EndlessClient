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

        public int X { get; }

        public int Y { get; }

        public EODirection Direction { get; }

        public NPCFrame Frame { get; }

        public int ActualAttackFrame { get; }

        public Option<int> OpponentID { get; }
    }
}