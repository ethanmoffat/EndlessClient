using System.Collections.Generic;
using Amadevus.RecordGenerator;

namespace EOLib.Domain.Interact.Skill
{
    [Record]
    public sealed partial class Skill
    {
        public int Id { get; }

        public int LevelRequirement { get; }

        public int ClassRequirement { get; }

        public int GoldRequirement { get; }

        public IReadOnlyList<int> SkillRequirements { get; }

        public int StrRequirement { get; }

        public int IntRequirement { get; }

        public int WisRequirement { get; }

        public int AgiRequirement { get; }

        public int ConRequirement { get; }

        public int ChaRequirement { get; }
    }

}
