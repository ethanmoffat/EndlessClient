// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Character
{
	public class CharacterStats : ICharacterStats
	{
		public IReadOnlyDictionary<CharacterStat, int> Stats { get; private set; }

		public int this[CharacterStat stat] { get { return Stats[stat]; } }

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
			var stats = new Dictionary<CharacterStat, int>(23);

			var allStatTypes = Enum.GetValues(typeof(CharacterStat));
			foreach (CharacterStat stat in allStatTypes)
				stats.Add(stat, 0);

			return stats;
		}
	}
}
