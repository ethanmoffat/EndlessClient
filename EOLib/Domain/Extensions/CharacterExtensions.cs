using EOLib.Domain.Character;

namespace EOLib.Domain.Extensions
{
    public static class CharacterExtensions
    {
        //This was taken from OldCharacter.ApplyData (before function was removed)
        public static ICharacter WithAppliedData(this ICharacter original, ICharacter updatedData)
        {
            var existingRenderProps = original.RenderProperties;
            var newRenderProps = existingRenderProps
                .WithBootsGraphic(updatedData.RenderProperties.BootsGraphic)
                .WithArmorGraphic(updatedData.RenderProperties.ArmorGraphic)
                .WithHatGraphic(updatedData.RenderProperties.HatGraphic)
                .WithShieldGraphic(updatedData.RenderProperties.ShieldGraphic)
                .WithWeaponGraphic(updatedData.RenderProperties.WeaponGraphic)
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
    }
}
