using Amadevus.RecordGenerator;

namespace EOLib.Domain.Party
{
    [Record(Features.ObjectEquals | Features.Withers | Features.Builder | Features.Constructor | Features.ToString)]
    public sealed partial class PartyMember
    {
        public short CharacterID { get; }

        public bool IsLeader { get; }

        public byte Level { get; }

        public byte PercentHealth { get; }

        public string Name { get; }
    }
}
