using Amadevus.RecordGenerator;
using EOLib.Domain.Extensions;
using EOLib.Domain.Spells;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Character
{
    [Record]
    public sealed partial class Character : ISpellTargetable
    {
        private static readonly Character _default = new Builder
        {
            Stats = new CharacterStats(),
            RenderProperties = new CharacterRenderProperties.Builder().ToImmutable()
        }.ToImmutable();

        public static Character Default => _default;

        public int ID { get; }

        public int Index => ID;

        public int X => RenderProperties.IsActing(CharacterActionState.Walking)
            ? RenderProperties.GetDestinationX()
            : RenderProperties.MapX;

        public int Y => RenderProperties.IsActing(CharacterActionState.Walking)
            ? RenderProperties.GetDestinationY()
            : RenderProperties.MapY;

        public string Name { get; }

        public string Title { get; }

        public string GuildName { get; }

        public string GuildRank { get; }

        public string GuildTag { get; }

        public int ClassID { get; }

        public AdminLevel AdminLevel { get; }

        public CharacterRenderProperties RenderProperties { get; }

        public CharacterStats Stats { get; }

        public int MapID { get; }

        public bool NoWall { get; }

        public static Character FromCharacterSelectionListEntry(CharacterSelectionListEntry selectionListEntry)
        {
            return new Builder
            {
                Name = selectionListEntry.Name,
                ID = selectionListEntry.Id,
                Stats = new CharacterStats()
                    .WithNewStat(CharacterStat.Level, selectionListEntry.Level),
                RenderProperties = new CharacterRenderProperties.Builder
                {
                    Gender = (int)selectionListEntry.Gender,
                    HairStyle = selectionListEntry.HairStyle,
                    HairColor = selectionListEntry.HairColor,
                    Race = selectionListEntry.Skin,
                    BootsGraphic = selectionListEntry.Equipment.Boots,
                    ArmorGraphic = selectionListEntry.Equipment.Armor,
                    HatGraphic = selectionListEntry.Equipment.Hat,
                    ShieldGraphic = selectionListEntry.Equipment.Shield,
                    WeaponGraphic = selectionListEntry.Equipment.Weapon,
                }.ToImmutable(),
                AdminLevel = selectionListEntry.Admin,
            }.ToImmutable();
        }

        public static Character FromNearby(CharacterMapInfo characterMapInfo)
        {
            return new Builder
            {
                Name = characterMapInfo.Name,
                ID = characterMapInfo.PlayerId,
                ClassID = characterMapInfo.ClassId,
                MapID = characterMapInfo.MapId,
                GuildTag = characterMapInfo.GuildTag,
                Stats = CharacterStats.FromCharacterMapInfo(characterMapInfo),
                RenderProperties = CharacterRenderProperties.FromCharacterMapInfo(characterMapInfo),
            }.ToImmutable();
        }
    }
}