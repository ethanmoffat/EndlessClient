using EOLib.Domain.Character;

namespace EOLib.Domain.Extensions
{
    public static class CharacterStatExtensions
    {
        public static string GetKarmaString(this ICharacterStats characterStats)
        {
            /* 0    - 100  = Demonic
             * 101  - 500  = Doomed
             * 501  - 750  = Cursed
             * 751  - 900  = Evil
             * 901  - 1099 = Neutral
             * 1100 - 1249 = Good
             * 1250 - 1499 = Blessed
             * 1500 - 1899 = Saint
             * 1900 - 2000 = Pure
             */

            var num = characterStats[CharacterStat.Karma];

            if (num >= 0)
            {
                if (num <= 100)
                    return "Demonic";
                if (num <= 500)
                    return "Doomed";
                if (num <= 750)
                    return "Cursed";
                if (num <= 900)
                    return "Evil";
                if (num <= 1099)
                    return "Neutral";
                if (num <= 1249)
                    return "Good";
                if (num <= 1499)
                    return "Blessed";
                if (num <= 1899)
                    return "Saint";
                if (num <= 2000)
                    return "Pure";
            }

            return string.Empty;
        }
    }
}
