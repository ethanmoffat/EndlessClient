using Amadevus.RecordGenerator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Character
{
    [Record(Features.Constructor | Features.ToString | Features.ObjectEquals)]
    public sealed partial class CharacterStats
    {
        public IReadOnlyDictionary<CharacterStat, int> Stats { get; }

        public int this[CharacterStat stat] => Stats[stat];

        public CharacterStats() =>
            Stats = ((IEnumerable<CharacterStat>)Enum.GetValues(typeof(CharacterStat))).ToDictionary(k => k, v => 0);

        public CharacterStats WithNewStat(CharacterStat whichStat, int statValue)
        {
            var newStats = Stats.ToDictionary(x => x.Key, x => x.Value);
            newStats[whichStat] = statValue;
            return new CharacterStats(newStats);
        }
    }
}
