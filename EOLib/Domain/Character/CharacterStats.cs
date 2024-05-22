using Amadevus.RecordGenerator;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
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

        public CharacterStats Merge(CharacterStats other) => new CharacterStats(Stats.Concat(other.Stats).ToDictionary(k => k.Key, v => v.Value));

        public static CharacterStats FromSelectCharacterData(WelcomeReplyServerPacket.WelcomeCodeDataSelectCharacter selectCharacterData)
        {
            var characterStatsWelcome = selectCharacterData.Stats;
            var characterStats = new Dictionary<CharacterStat, int>
            {
                [CharacterStat.Level] = selectCharacterData.Level,
                [CharacterStat.Experience] = selectCharacterData.Experience,
                [CharacterStat.Usage] = selectCharacterData.Usage,
                [CharacterStat.HP] = characterStatsWelcome.Hp,
                [CharacterStat.MaxHP] = characterStatsWelcome.MaxHp,
                [CharacterStat.TP] = characterStatsWelcome.Tp,
                [CharacterStat.MaxTP] = characterStatsWelcome.MaxTp,
                [CharacterStat.SP] = characterStatsWelcome.MaxSp,
                [CharacterStat.MaxSP] = characterStatsWelcome.MaxSp,
                [CharacterStat.StatPoints] = characterStatsWelcome.StatPoints,
                [CharacterStat.SkillPoints] = characterStatsWelcome.SkillPoints,
                [CharacterStat.Karma] = characterStatsWelcome.Karma,
                [CharacterStat.Strength] = characterStatsWelcome.Base.Str,
                [CharacterStat.Intelligence] = characterStatsWelcome.Base.Intl,
                [CharacterStat.Wisdom] = characterStatsWelcome.Base.Wis,
                [CharacterStat.Agility] = characterStatsWelcome.Base.Agi,
                [CharacterStat.Constitution] = characterStatsWelcome.Base.Con,
                [CharacterStat.Charisma] = characterStatsWelcome.Base.Cha,
                [CharacterStat.MinDam] = characterStatsWelcome.Secondary.MinDamage,
                [CharacterStat.MaxDam] = characterStatsWelcome.Secondary.MaxDamage,
                [CharacterStat.Accuracy] = characterStatsWelcome.Secondary.Accuracy,
                [CharacterStat.Evade] = characterStatsWelcome.Secondary.Evade,
                [CharacterStat.Armor] = characterStatsWelcome.Secondary.Armor,
            };

            return new CharacterStats(characterStats);
        }

        public static CharacterStats FromCharacterMapInfo(CharacterMapInfo characterMapInfo)
        {
            return new CharacterStats(new Dictionary<CharacterStat, int>
            {
                [CharacterStat.Level] = characterMapInfo.Level,
                [CharacterStat.HP] = characterMapInfo.Hp,
                [CharacterStat.MaxHP] = characterMapInfo.MaxHp,
                [CharacterStat.TP] = characterMapInfo.Tp,
                [CharacterStat.MaxTP] = characterMapInfo.MaxTp,
            });
        }

        public static CharacterStats FromStatUpdate(CharacterStatsUpdate characterStatsUpdate)
        {
            return new CharacterStats(new Dictionary<CharacterStat, int>
            {
                [CharacterStat.MaxHP] = characterStatsUpdate.MaxHp,
                [CharacterStat.MaxTP] = characterStatsUpdate.MaxTp,
                [CharacterStat.MaxSP] = characterStatsUpdate.MaxSp,
                [CharacterStat.Strength] = characterStatsUpdate.BaseStats.Str,
                [CharacterStat.Intelligence] = characterStatsUpdate.BaseStats.Intl,
                [CharacterStat.Wisdom] = characterStatsUpdate.BaseStats.Wis,
                [CharacterStat.Agility] = characterStatsUpdate.BaseStats.Agi,
                [CharacterStat.Constitution] = characterStatsUpdate.BaseStats.Con,
                [CharacterStat.Charisma] = characterStatsUpdate.BaseStats.Cha,
                [CharacterStat.MaxWeight] = characterStatsUpdate.MaxWeight,
                [CharacterStat.MinDam] = characterStatsUpdate.SecondaryStats.MinDamage,
                [CharacterStat.MaxDam] = characterStatsUpdate.SecondaryStats.MaxDamage,
                [CharacterStat.Accuracy] = characterStatsUpdate.SecondaryStats.Accuracy,
                [CharacterStat.Evade] = characterStatsUpdate.SecondaryStats.Evade,
                [CharacterStat.Armor] = characterStatsUpdate.SecondaryStats.Armor,
            });
        }

        public static CharacterStats FromStatReset(CharacterStatsReset characterStatsUpdate)
        {
            return new CharacterStats(new Dictionary<CharacterStat, int>
            {
                [CharacterStat.StatPoints] = characterStatsUpdate.StatPoints,
                [CharacterStat.SkillPoints] = characterStatsUpdate.SkillPoints,
                [CharacterStat.HP] = characterStatsUpdate.Hp,
                [CharacterStat.MaxHP] = characterStatsUpdate.MaxHp,
                [CharacterStat.TP] = characterStatsUpdate.Tp,
                [CharacterStat.MaxTP] = characterStatsUpdate.MaxTp,
                [CharacterStat.MaxSP] = characterStatsUpdate.MaxSp,
                [CharacterStat.Strength] = characterStatsUpdate.Base.Str,
                [CharacterStat.Intelligence] = characterStatsUpdate.Base.Intl,
                [CharacterStat.Wisdom] = characterStatsUpdate.Base.Wis,
                [CharacterStat.Agility] = characterStatsUpdate.Base.Agi,
                [CharacterStat.Constitution] = characterStatsUpdate.Base.Con,
                [CharacterStat.Charisma] = characterStatsUpdate.Base.Cha,
                [CharacterStat.MinDam] = characterStatsUpdate.Secondary.MinDamage,
                [CharacterStat.MaxDam] = characterStatsUpdate.Secondary.MaxDamage,
                [CharacterStat.Accuracy] = characterStatsUpdate.Secondary.Accuracy,
                [CharacterStat.Evade] = characterStatsUpdate.Secondary.Evade,
                [CharacterStat.Armor] = characterStatsUpdate.Secondary.Armor,
            });
        }
    }
}
