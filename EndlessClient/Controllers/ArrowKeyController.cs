// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Character;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;

namespace EndlessClient.Controllers
{
    public class ArrowKeyController : IArrowKeyController
    {
        private readonly ICharacterWalkValidationActions _characterWalkValidationActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ICharacterActions _characterActions;
        private readonly ICharacterProvider _characterProvider;

        public ArrowKeyController(ICharacterWalkValidationActions characterWalkValidationActions,
                                  ICharacterAnimationActions characterAnimationActions,
                                  ICharacterActions characterActions,
                                  ICharacterProvider characterProvider)
        {
            _characterWalkValidationActions = characterWalkValidationActions;
            _characterAnimationActions = characterAnimationActions;
            _characterActions = characterActions;
            _characterProvider = characterProvider;
        }

        public bool MoveLeft()
        {
            if (!CurrentActionIsStanding())
                return false;

            FaceOrAttemptWalk(EODirection.Left);

            return true;
        }

        public bool MoveRight()
        {
            if (!CurrentActionIsStanding())
                return false;

            FaceOrAttemptWalk(EODirection.Right);

            return true;
        }

        public bool MoveUp()
        {
            if (!CurrentActionIsStanding())
                return false;

            FaceOrAttemptWalk(EODirection.Up);

            return true;
        }

        public bool MoveDown()
        {
            if (!CurrentActionIsStanding())
                return false;

            FaceOrAttemptWalk(EODirection.Down);

            return true;
        }

        private bool CurrentActionIsStanding()
        {
            return _characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing);
        }

        private bool CurrentDirectionIs(EODirection direction)
        {
            return _characterProvider.MainCharacter.RenderProperties.IsFacing(direction);
        }

        private void FaceOrAttemptWalk(EODirection direction)
        {
            if (!CurrentDirectionIs(direction))
                FaceDirection(direction);
            else
                AttemptToStartWalking();
        }

        private void FaceDirection(EODirection direction)
        {
            _characterActions.Face(direction);
            _characterAnimationActions.Face(direction);
        }

        private void AttemptToStartWalking()
        {
            if (!_characterWalkValidationActions.CanMoveToDestinationCoordinates())
            {
                //todo: handle based on cell state
            }
            else
            {
                _characterActions.Walk();
                _characterAnimationActions.StartWalking();
            }
        }
    }
}
