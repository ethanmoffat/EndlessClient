using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.HUD;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Localization;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(IControlKeyController))]
    public class ControlKeyController : IControlKeyController
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly IAttackValidationActions _attackValidationActions;
        private readonly ICharacterActions _characterActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ISfxPlayer _sfxPlayer;

        public ControlKeyController(ICharacterProvider characterProvider,
                                    IAttackValidationActions attackValidationActions,
                                    ICharacterActions characterActions,
                                    ICharacterAnimationActions characterAnimationActions,
                                    IStatusLabelSetter statusLabelSetter,
                                    ISfxPlayer sfxPlayer)
        {
            _characterProvider = characterProvider;
            _attackValidationActions = attackValidationActions;
            _characterActions = characterActions;
            _characterAnimationActions = characterAnimationActions;
            _statusLabelSetter = statusLabelSetter;
            _sfxPlayer = sfxPlayer;
        }

        public bool Attack()
        {
            _characterAnimationActions.CancelClickToWalk();

            if (!CanAttackAgain())
                return false;

            AttemptAttack();

            return true;
        }

        private bool CanAttackAgain()
        {
            var rp = _characterProvider.MainCharacter.RenderProperties;
            return rp.IsActing(CharacterActionState.Standing) ||
                   rp.RenderAttackFrame == CharacterRenderProperties.MAX_NUMBER_OF_ATTACK_FRAMES;
        }

        private void AttemptAttack()
        {
            var showAnimationAnyway = false;
            var validationResult = _attackValidationActions.ValidateCharacterStateBeforeAttacking();
            if (validationResult != AttackValidationError.OK)
            {
                if (validationResult == AttackValidationError.Overweight)
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                                                      EOResourceID.STATUS_LABEL_CANNOT_ATTACK_OVERWEIGHT);
                else if (validationResult == AttackValidationError.Exhausted)
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                                                      EOResourceID.ATTACK_YOU_ARE_EXHAUSTED_SP);
                else if (validationResult == AttackValidationError.NotYourBattle)
                {
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION,
                                                      EOResourceID.STATUS_LABEL_UNABLE_TO_ATTACK);
                    showAnimationAnyway = true;
                }
                else if (validationResult == AttackValidationError.MissingArrows)
                {
                    _sfxPlayer.PlaySfx(SoundEffectID.NoArrows);
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                                                      EOResourceID.STATUS_LABEL_YOU_HAVE_NO_ARROWS);

                    _sfxPlayer.PlaySfx(SoundEffectID.NoArrows);
                }
            }
            else
                showAnimationAnyway = true;

            if (showAnimationAnyway)
            {
                _characterActions.Attack();
                _characterAnimationActions.StartAttacking();
            }
        }
    }

    public interface IControlKeyController
    {
        bool Attack();
    }
}