using EOLib.Domain.Character;
using System;

namespace EOLib.Domain.Extensions
{
    public static class CharacterExtensions
    {
        public static ICharacter WithAppliedData(this ICharacter original, ICharacter updatedData, bool isRangedWeapon)
        {
            var existingRenderProps = original.RenderProperties;
            var newRenderProps = existingRenderProps
                .WithBootsGraphic(updatedData.RenderProperties.BootsGraphic)
                .WithArmorGraphic(updatedData.RenderProperties.ArmorGraphic)
                .WithHatGraphic(updatedData.RenderProperties.HatGraphic)
                .WithShieldGraphic(updatedData.RenderProperties.ShieldGraphic)
                .WithWeaponGraphic(updatedData.RenderProperties.WeaponGraphic, isRangedWeapon)
                .WithDirection(updatedData.RenderProperties.Direction)
                .WithHairStyle(updatedData.RenderProperties.HairStyle)
                .WithHairColor(updatedData.RenderProperties.HairColor)
                .WithGender(updatedData.RenderProperties.Gender)
                .WithRace(updatedData.RenderProperties.Race)
                .WithSitState(updatedData.RenderProperties.SitState)
                .WithMapX(updatedData.RenderProperties.MapX)
                .WithMapY(updatedData.RenderProperties.MapY)
                .ResetAnimationFrames();

            var existingStats = original.Stats;
            var newStats = existingStats
                .WithNewStat(CharacterStat.Level, existingStats[CharacterStat.Level])
                .WithNewStat(CharacterStat.HP, existingStats[CharacterStat.HP])
                .WithNewStat(CharacterStat.MaxHP, existingStats[CharacterStat.MaxHP])
                .WithNewStat(CharacterStat.TP, existingStats[CharacterStat.TP])
                .WithNewStat(CharacterStat.MaxTP, existingStats[CharacterStat.MaxTP]);

            return original
                .WithName(updatedData.Name)
                .WithGuildTag(updatedData.GuildTag)
                .WithMapID(updatedData.MapID)
                .WithRenderProperties(newRenderProps)
                .WithStats(newStats);
        }

        public static ICharacter WithDamage(this ICharacter original, int damageTaken, bool isDead)
        {
            var stats = original.Stats;
            stats = stats.WithNewStat(CharacterStat.HP, (short)Math.Max(stats[CharacterStat.HP] - damageTaken, 0));

            var props = original.RenderProperties;
            if (isDead)
                props = props.WithDead();

            return original.WithStats(stats).WithRenderProperties(props);
        }
    }
}
