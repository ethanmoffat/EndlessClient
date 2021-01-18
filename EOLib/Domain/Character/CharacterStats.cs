using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Character
{
    public class CharacterStats : ICharacterStats
    {
        public IReadOnlyDictionary<CharacterStat, int> Stats { get; }

        public int this[CharacterStat stat] => Stats[stat];

        public CharacterStats()
        {
            Stats = CreateStatCollection();
        }

        private CharacterStats(IReadOnlyDictionary<CharacterStat, int> stats)
        {
            Stats = stats;
        }

        public ICharacterStats WithNewStat(CharacterStat whichStat, int statValue)
        {
            var newStats = Stats.ToDictionary(x => x.Key, x => x.Value);
            newStats[whichStat] = statValue;
            return new CharacterStats(newStats);
        }

        private static IReadOnlyDictionary<CharacterStat, int> CreateStatCollection()
        {
            var stats = new Dictionary<CharacterStat, int>(25);

            var allStatTypes = Enum.GetValues(typeof(CharacterStat));
            foreach (CharacterStat stat in allStatTypes)
                stats.Add(stat, 0);

            return stats;
        }

        public override bool Equals(object obj)
        {
            return obj is CharacterStats stats &&
                   EqualityComparer<IReadOnlyDictionary<CharacterStat, int>>.Default.Equals(Stats, stats.Stats);
        }

        public override int GetHashCode()
        {
            return -1464643476 + EqualityComparer<IReadOnlyDictionary<CharacterStat, int>>.Default.GetHashCode(Stats);
        }
    }

    public interface ICharacterStats
    {
        IReadOnlyDictionary<CharacterStat, int> Stats { get; }

        int this[CharacterStat stat] { get; }

        ICharacterStats WithNewStat(CharacterStat whichStat, int statValue);
    }
}
