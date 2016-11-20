// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;

namespace EndlessClient.Controllers
{
    public class ArrowKeyController : IArrowKeyController
    {
        private readonly IWalkValidationActions _walkValidationActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ICharacterActions _characterActions;
        private readonly ICharacterProvider _characterProvider;
        private IWalkErrorHandler _walkErrorHandler;

        public ArrowKeyController(IWalkValidationActions walkValidationActions,
                                  ICharacterAnimationActions characterAnimationActions,
                                  ICharacterActions characterActions,
                                  ICharacterProvider characterProvider,
                                  IWalkErrorHandler walkErrorHandler)
        {
            _walkValidationActions = walkValidationActions;
            _characterAnimationActions = characterAnimationActions;
            _characterActions = characterActions;
            _characterProvider = characterProvider;
            _walkErrorHandler = walkErrorHandler;
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
            if (!_walkValidationActions.CanMoveToDestinationCoordinates())
            {
                _walkErrorHandler.HandleWalkError();
            }
            else
            {
                _characterActions.Walk();
                _characterAnimationActions.StartWalking();
            }
        }
    }

    public interface IArrowKeyController
    {
        bool MoveLeft();

        bool MoveRight();

        bool MoveUp();

        bool MoveDown();
    }
}
