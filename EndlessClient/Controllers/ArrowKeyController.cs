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
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ICharacterProvider _characterProvider;

        public ArrowKeyController(ICharacterAnimationActions characterAnimationActions,
                                  ICharacterProvider characterProvider)
        {
            _characterAnimationActions = characterAnimationActions;
            _characterProvider = characterProvider;
        }

        public bool MoveLeft()
        {
            if (!CurrentActionIsStanding())
                return false;

            FaceOrStartWalking(EODirection.Left);

            return true;
        }

        public bool MoveRight()
        {
            if (!CurrentActionIsStanding())
                return false;

            FaceOrStartWalking(EODirection.Right);

            return true;
        }

        public bool MoveUp()
        {
            if (!CurrentActionIsStanding())
                return false;

            FaceOrStartWalking(EODirection.Up);

            return true;
        }

        public bool MoveDown()
        {
            if (!CurrentActionIsStanding())
                return false;

            FaceOrStartWalking(EODirection.Down);

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

        private void FaceOrStartWalking(EODirection direction)
        {
            if (!CurrentDirectionIs(direction))
                _characterAnimationActions.Face(direction);
            else
                _characterAnimationActions.StartWalking();
        }
    }
}
