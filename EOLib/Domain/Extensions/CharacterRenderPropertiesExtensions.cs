using System.Linq;
using EOLib.Domain.Character;
using EOLib.Domain.Map;

namespace EOLib.Domain.Extensions
{
    public static class CharacterRenderPropertiesExtensions
    {
        public static bool IsFacing(this CharacterRenderProperties renderProperties, params EODirection[] directions)
        {
            return directions.Contains(renderProperties.Direction);
        }

        public static bool IsActing(this CharacterRenderProperties renderProperties, params CharacterActionState[] actions)
        {
            return actions.Contains(renderProperties.CurrentAction);
        }

        public static int GetDestinationX(this CharacterRenderProperties renderProperties)
        {
            var offset = GetXOffset(renderProperties.Direction);
            return renderProperties.MapX + offset;
        }

        public static int GetDestinationY(this CharacterRenderProperties renderProperties)
        {
            var offset = GetYOffset(renderProperties.Direction);
            return renderProperties.MapY + offset;
        }

        private static int GetXOffset(EODirection direction)
        {
            return direction == EODirection.Right ? 1 :
                   direction == EODirection.Left ? -1 : 0;
        }

        private static int GetYOffset(EODirection direction)
        {
            return direction == EODirection.Down ? 1 :
                   direction == EODirection.Up ? -1 : 0;
        }

        public static CharacterRenderProperties WithNextWalkFrame(this CharacterRenderProperties rp, bool isSteppingStone = false)
        {
            var props = rp.ToBuilder();
            props.ActualWalkFrame = (props.ActualWalkFrame + 1) % CharacterRenderProperties.MAX_NUMBER_OF_WALK_FRAMES;

            if (isSteppingStone && props.ActualWalkFrame > 1)
            {
                // force first walk frame when on a stepping stone (this is the graphic used for jump, Y adjusted)
                props.RenderWalkFrame = 1;
            }
            else
            {
                props.RenderWalkFrame = props.ActualWalkFrame;
            }

            props.CurrentAction = props.RenderWalkFrame == 0 ? CharacterActionState.Standing : CharacterActionState.Walking;

            return props.ToImmutable();
        }

        public static CharacterRenderProperties WithNextAttackFrame(this CharacterRenderProperties rp, bool isRangedWeapon)
        {
            var props = rp.ToBuilder();
            props.ActualAttackFrame = (props.ActualAttackFrame + 1) % CharacterRenderProperties.MAX_NUMBER_OF_WALK_FRAMES;
            props.RenderAttackFrame = props.ActualAttackFrame;

            if (isRangedWeapon)
            {
                // ranged attack ticks: 0 0 1 1 1
                props.RenderAttackFrame /= CharacterRenderProperties.MAX_NUMBER_OF_RANGED_ATTACK_FRAMES;
                props.RenderAttackFrame = System.Math.Min(props.RenderAttackFrame, CharacterRenderProperties.MAX_NUMBER_OF_RANGED_ATTACK_FRAMES - 1);
            }
            else
            {
                // melee attack ticks:  0 1 2 2 2
                props.RenderAttackFrame = System.Math.Min(props.RenderAttackFrame, CharacterRenderProperties.MAX_NUMBER_OF_ATTACK_FRAMES - 1);
            }

            props.CurrentAction = props.ActualAttackFrame == 0 ? CharacterActionState.Standing : CharacterActionState.Attacking;

            return props.ToImmutable();
        }

        public static CharacterRenderProperties WithNextEmoteFrame(this CharacterRenderProperties rp)
        {
            var props = rp.ToBuilder();
            props.EmoteFrame = (props.EmoteFrame + 1) % CharacterRenderProperties.MAX_NUMBER_OF_EMOTE_FRAMES;

            var resetAction = props.SitState == SitState.Standing ? CharacterActionState.Standing : CharacterActionState.Sitting;
            props.CurrentAction = props.EmoteFrame == 0
                ? resetAction
                : props.CurrentAction == CharacterActionState.Attacking // when using an instrument keep the current state as "Attacking"
                    ? CharacterActionState.Attacking
                    : CharacterActionState.Emote;
            return props.ToImmutable();
        }

        public static CharacterRenderProperties WithNextSpellCastFrame(this CharacterRenderProperties rp)
        {
            // spell cast frame ticks: 0 0 1 1 1
            var props = rp.ToBuilder();
            props.ActualSpellCastFrame = (props.ActualSpellCastFrame + 1) % CharacterRenderProperties.MAX_NUMBER_OF_WALK_FRAMES;
            props.CurrentAction = props.ActualSpellCastFrame == 0 ? CharacterActionState.Standing : CharacterActionState.SpellCast;
            return props.ToImmutable();
        }

        public static CharacterRenderProperties ResetAnimationFrames(this CharacterRenderProperties rp)
        {
            var props = rp.ToBuilder();
            props.RenderWalkFrame = 0;
            props.ActualWalkFrame = 0;
            props.RenderAttackFrame = 0;
            props.EmoteFrame = 0;
            props.CurrentAction = props.SitState == SitState.Standing ? CharacterActionState.Standing : CharacterActionState.Sitting;
            return props.ToImmutable();
        }

        public static MapCoordinate Coordinates(this CharacterRenderProperties rp) => new MapCoordinate(rp.MapX, rp.MapY);
    }
}
