using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.HUD;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Localization;
using Optional;

namespace EndlessClient.Controllers;

[AutoMappedType]
public class ArrowKeyController : IArrowKeyController
{
    private readonly IWalkValidationActions _walkValidationActions;
    private readonly ICharacterAnimationActions _characterAnimationActions;
    private readonly ICharacterProvider _characterProvider;
    private readonly IUnwalkableTileActions _unwalkableTileActions;
    private readonly IUnwalkableTileActionsHandler _unwalkableTileActionsHandler;
    private readonly IStatusLabelSetter _statusLabelSetter;
    private readonly IGhostingRepository _ghostingRepository;
    private readonly ISfxPlayer _sfxPlayer;

    public ArrowKeyController(IWalkValidationActions walkValidationActions,
                              ICharacterAnimationActions characterAnimationActions,
                              ICharacterProvider characterProvider,
                              IUnwalkableTileActions walkErrorHandler,
                              IUnwalkableTileActionsHandler unwalkableTileActionsHandler,
                              IStatusLabelSetter statusLabelSetter,
                              IGhostingRepository ghostingRepository,
                              ISfxPlayer sfxPlayer)
    {
        _walkValidationActions = walkValidationActions;
        _characterAnimationActions = characterAnimationActions;
        _characterProvider = characterProvider;
        _unwalkableTileActions = walkErrorHandler;
        _unwalkableTileActionsHandler = unwalkableTileActionsHandler;
        _statusLabelSetter = statusLabelSetter;
        _ghostingRepository = ghostingRepository;
        _sfxPlayer = sfxPlayer;
    }

    public bool MoveLeft()
    {
        if (!CanWalkAgain())
            return false;

        FaceOrAttemptWalk(EODirection.Left);

        return true;
    }

    public bool MoveRight()
    {
        if (!CanWalkAgain())
            return false;

        FaceOrAttemptWalk(EODirection.Right);

        return true;
    }

    public bool MoveUp()
    {
        if (!CanWalkAgain())
            return false;

        FaceOrAttemptWalk(EODirection.Up);

        return true;
    }

    public bool MoveDown()
    {
        if (!CanWalkAgain())
            return false;

        FaceOrAttemptWalk(EODirection.Down);

        return true;
    }

    public void KeysUp()
    {
        _ghostingRepository.ResetState();
    }

    private bool CanWalkAgain()
    {
        return _characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing) ||
               _characterProvider.MainCharacter.RenderProperties.ActualWalkFrame == CharacterRenderProperties.MAX_NUMBER_OF_WALK_FRAMES - 1;
    }

    private bool CurrentDirectionIs(EODirection direction)
    {
        return _characterProvider.MainCharacter.RenderProperties.IsFacing(direction);
    }

    private void FaceOrAttemptWalk(EODirection direction)
    {
        if (!CurrentDirectionIs(direction) && _characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing))
        {
            _characterAnimationActions.Face(direction);
        }
        else
        {
            _characterAnimationActions.Face(direction);
            AttemptToStartWalking();
        }
    }

    private void AttemptToStartWalking()
    {
        var walkValidationResult = _walkValidationActions.CanMoveToDestinationCoordinates();
        if (walkValidationResult == WalkValidationResult.GhostComplete)
            _sfxPlayer.PlaySfx(SoundEffectID.GhostPlayer);

        switch (walkValidationResult)
        {
            case WalkValidationResult.BlockedByCharacter:
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_KEEP_MOVING_THROUGH_PLAYER);
                break;
            case WalkValidationResult.NotWalkable:
                var (unwalkableActions, cellState) = _unwalkableTileActions.GetUnwalkableTileActions();
                _unwalkableTileActionsHandler.HandleUnwalkableTileActions(unwalkableActions, cellState);
                break;
            case WalkValidationResult.GhostComplete:
                _characterAnimationActions.StartWalking(Option.None<MapCoordinate>(), ghosted: true);
                break;
            case WalkValidationResult.Walkable:
                _characterAnimationActions.StartWalking(Option.None<MapCoordinate>());
                break;
        }
    }
}

public interface IArrowKeyController
{
    bool MoveLeft();

    bool MoveRight();

    bool MoveUp();

    bool MoveDown();

    void KeysUp();
}