using Amadevus.RecordGenerator;
using EOLib.Domain.Extensions;
using EOLib.Domain.Spells;

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
    }
}