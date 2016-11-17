// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.HUD;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Localization;

namespace EndlessClient.Controllers
{
    public class ControlKeyController : IControlKeyController
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly IAttackValidationActions _attackValidationActions;
        private readonly ICharacterActions _characterActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly IStatusLabelSetter _statusLabelSetter;

        public ControlKeyController(ICharacterProvider characterProvider,
                                    IAttackValidationActions attackValidationActions,
                                    ICharacterActions characterActions,
                                    ICharacterAnimationActions characterAnimationActions,
                                    IStatusLabelSetter statusLabelSetter)
        {
            _characterProvider = characterProvider;
            _attackValidationActions = attackValidationActions;
            _characterActions = characterActions;
            _characterAnimationActions = characterAnimationActions;
            _statusLabelSetter = statusLabelSetter;
        }

        public bool Attack()
        {
            if (!CurrentActionIsStanding())
                return false;

            AttemptAttack();

            return true;
        }

        private bool CurrentActionIsStanding()
        {
            return _characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing);
        }

        private void AttemptAttack()
        {
            var validationResult = _attackValidationActions.ValidateCharacterStateBeforeAttacking();
            if (validationResult != AttackValidationError.OK)
            {
                if (validationResult == AttackValidationError.Overweight)
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                                                      EOResourceID.STATUS_LABEL_CANNOT_ATTACK_OVERWEIGHT);
                else if(validationResult == AttackValidationError.Exhausted)
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                                                      EOResourceID.ATTACK_YOU_ARE_EXHAUSTED_SP);
                else if(validationResult == AttackValidationError.NotYourBattle)
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION,
                                                      EOResourceID.STATUS_LABEL_UNABLE_TO_ATTACK);
            }
            else
            {
                //todo: lower SP for character when attacking
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
