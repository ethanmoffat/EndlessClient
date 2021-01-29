using AutomaticTypeMapper;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(IArrowKeyController))]
    public class ArrowKeyController : IArrowKeyController
    {
        private readonly IWalkValidationActions _walkValidationActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ICharacterActions _characterActions;
        private readonly ICharacterProvider _characterProvider;
        private readonly IUnwalkableTileActions _unwalkableTileActions;
        private readonly ISpikeTrapActions _spikeTrapActions;

        public ArrowKeyController(IWalkValidationActions walkValidationActions,
                                  ICharacterAnimationActions characterAnimationActions,
                                  ICharacterActions characterActions,
                                  ICharacterProvider characterProvider,
                                  IUnwalkableTileActions walkErrorHandler,
                                  ISpikeTrapActions spikeTrapActions)
        {
            _walkValidationActions = walkValidationActions;
            _characterAnimationActions = characterAnimationActions;
            _characterActions = characterActions;
            _characterProvider = characterProvider;
            _unwalkableTileActions = walkErrorHandler;
            _spikeTrapActions = spikeTrapActions;
        }

        public bool MoveLeft(bool faceAndMove = false)
        {
            if (!CanWalkAgain())
                return false;

            FaceOrAttemptWalk(EODirection.Left, faceAndMove);

            return true;
        }

        public bool MoveRight(bool faceAndMove = false)
        {
            if (!CanWalkAgain())
                return false;

            FaceOrAttemptWalk(EODirection.Right, faceAndMove);

            return true;
        }

        public bool MoveUp(bool faceAndMove = false)
        {
            if (!CanWalkAgain())
                return false;

            FaceOrAttemptWalk(EODirection.Up, faceAndMove);

            return true;
        }

        public bool MoveDown(bool faceAndMove = false)
        {
            if (!CanWalkAgain())
                return false;

            FaceOrAttemptWalk(EODirection.Down, faceAndMove);

            return true;
        }

        private bool CanWalkAgain()
        {
            return _characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing) ||
                   _characterProvider.MainCharacter.RenderProperties.RenderWalkFrame == CharacterRenderProperties.MAX_NUMBER_OF_WALK_FRAMES;
        }

        private bool CurrentDirectionIs(EODirection direction)
        {
            return _characterProvider.MainCharacter.RenderProperties.IsFacing(direction);
        }

        private void FaceOrAttemptWalk(EODirection direction, bool faceAndMove)
        {
            if (!CurrentDirectionIs(direction))
            {
                FaceDirection(direction);
                if (faceAndMove)
                    AttemptToStartWalking();
            }
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
                _unwalkableTileActions.HandleUnwalkableTile();
            }
            else
            {
                _characterAnimationActions.StartWalking();
                _characterActions.Walk();

                var coordinate = new MapCoordinate(
                    _characterProvider.MainCharacter.RenderProperties.MapX,
                    _characterProvider.MainCharacter.RenderProperties.MapY);
                _spikeTrapActions.HideSpikeTrap(coordinate);

                coordinate = new MapCoordinate(
                    _characterProvider.MainCharacter.RenderProperties.GetDestinationX(),
                    _characterProvider.MainCharacter.RenderProperties.GetDestinationY());
                _spikeTrapActions.ShowSpikeTrap(coordinate);
            }
        }
    }

    public interface IArrowKeyController
    {
        bool MoveLeft(bool faceAndMove = false);

        bool MoveRight(bool faceAndMove = false);

        bool MoveUp(bool faceAndMove = false);

        bool MoveDown(bool faceAndMove = false);
    }
}
