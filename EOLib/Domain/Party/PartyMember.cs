using Amadevus.RecordGenerator;

namespace EOLib.Domain.Party
{
    [Record(Features.ObjectEquals | Features.Withers | Features.Builder | Features.Constructor | Features.ToString)]
    public sealed partial class PartyMember
    {
        public int CharacterID { get; }

        public bool IsLeader { get; }

        public int Level { get; }

        public int PercentHealth { get; }

        public string Name { get; }
    }
}