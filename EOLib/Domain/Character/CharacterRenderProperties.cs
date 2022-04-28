using Amadevus.RecordGenerator;

namespace EOLib.Domain.Character
{
    [Record]
    public sealed partial class CharacterRenderProperties
    {
        public const int MAX_NUMBER_OF_WALK_FRAMES          = 5;
        public const int MAX_NUMBER_OF_ATTACK_FRAMES        = 3;
        public const int MAX_NUMBER_OF_RANGED_ATTACK_FRAMES = 2;
        public const int MAX_NUMBER_OF_EMOTE_FRAMES         = 3;

        public CharacterActionState CurrentAction { get; }

        public byte HairStyle { get; }
        public byte HairColor { get; }
        public byte Race { get; }
        public byte Gender { get; }

        public short BootsGraphic { get; }
        public short ArmorGraphic { get; }
        public short HatGraphic { get; }
        public short ShieldGraphic { get; }
        public short WeaponGraphic { get; }

        public EODirection Direction { get; }
        public int MapX { get; }
        public int MapY { get; }

        public int ActualWalkFrame { get; }
        public int RenderWalkFrame { get; }
        public int ActualAttackFrame { get; }
        public int RenderAttackFrame { get; }
        public int EmoteFrame { get; }
        public int ActualSpellCastFrame { get; }

        public SitState SitState { get; }
        public Emote Emote { get; }

        public bool IsHidden { get; }
        public bool IsDead { get; }
        public bool IsDrunk { get; }

        public bool IsRangedWeapon { get; }
    }
}
