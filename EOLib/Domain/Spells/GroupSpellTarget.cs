using Amadevus.RecordGenerator;

namespace EOLib.Domain.Spells
{
    [Record]
    public sealed partial class GroupSpellTarget
    {
        public int TargetId { get; }

        public int PercentHealth { get; }

        public int TargetHp { get; }
    }
}
