using System;
using EOLib.Net.API;

namespace EOLib.Domain.Character
{
    public interface ICharacterRenderProperties : ICloneable
    {
        CharacterActionState CurrentAction { get; }

        byte HairStyle { get; }
        byte HairColor { get; }
        byte Race { get; }
        byte Gender { get; }

        short BootsGraphic { get; }
        short ArmorGraphic { get; }
        short HatGraphic { get; }
        short ShieldGraphic { get; }
        short WeaponGraphic { get; }

        EODirection Direction { get; }
        int MapX { get; }
        int MapY { get; }

        int WalkFrame { get; }
        int AttackFrame { get; }
        int EmoteFrame { get; }
        
        SitState SitState { get; }
        Emote Emote { get; }

        bool IsHidden { get; }
        bool IsDead { get; }

        ICharacterRenderProperties WithHairStyle(byte newHairStyle);
        ICharacterRenderProperties WithHairColor(byte newHairColor);
        ICharacterRenderProperties WithRace(byte newRace);
        ICharacterRenderProperties WithGender(byte newGender);

        ICharacterRenderProperties WithBootsGraphic(short bootsGraphic);
        ICharacterRenderProperties WithArmorGraphic(short armorGraphic);
        ICharacterRenderProperties WithHatGraphic(short hatGraphic);
        ICharacterRenderProperties WithShieldGraphic(short shieldGraphic);
        ICharacterRenderProperties WithWeaponGraphic(short weaponGraphic);

        ICharacterRenderProperties WithDirection(EODirection newDirection);
        ICharacterRenderProperties WithMapX(int mapX);
        ICharacterRenderProperties WithMapY(int mapY);

        ICharacterRenderProperties WithNextWalkFrame();
        ICharacterRenderProperties WithNextAttackFrame();
        ICharacterRenderProperties WithNextEmoteFrame();
        ICharacterRenderProperties WithNextSpellCastFrame();
        ICharacterRenderProperties ResetAnimationFrames();

        ICharacterRenderProperties WithSitState(SitState newState);
        ICharacterRenderProperties WithEmote(Emote emote);

        ICharacterRenderProperties WithIsHidden(bool hidden);
        ICharacterRenderProperties WithDead();
        ICharacterRenderProperties WithAlive();
    }
}
