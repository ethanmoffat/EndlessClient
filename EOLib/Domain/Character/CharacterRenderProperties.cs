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

        public int HairStyle { get; }
        public int HairColor { get; }
        public int Race { get; }
        public int Gender { get; }

        public int BootsGraphic { get; }
        public int ArmorGraphic { get; }
        public int HatGraphic { get; }
        public int ShieldGraphic { get; }
        public int WeaponGraphic { get; }

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
    }
}
