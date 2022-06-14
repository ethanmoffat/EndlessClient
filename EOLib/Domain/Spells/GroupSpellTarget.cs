using Amadevus.RecordGenerator;

namespace EOLib.Domain.Spells
{
    [Record]
    public sealed partial class GroupSpellTarget
    {
        public short TargetId { get; }

        public byte PercentHealth { get; }

        public short TargetHp { get; }
    }
}
