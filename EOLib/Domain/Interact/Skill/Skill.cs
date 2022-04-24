using Amadevus.RecordGenerator;
using System.Collections.Generic;

namespace EOLib.Domain.Interact.Skill
{
    [Record]
    public sealed partial class Skill
    {
        public short Id { get; }

        public byte LevelRequirement { get; }

        public byte ClassRequirement { get; }

        public int GoldRequirement { get; }

        public IReadOnlyList<short> SkillRequirements { get; }

        public short StrRequirement { get; }

        public short IntRequirement { get; }

        public short WisRequirement { get; }

        public short AgiRequirement { get; }

        public short ConRequirement { get; }

        public short ChaRequirement { get; }
    }

}
