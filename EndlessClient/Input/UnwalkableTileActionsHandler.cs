using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using System.Collections.Generic;

namespace EndlessClient.Input;

[AutoMappedType]
public class UnwalkableTileActionsHandler : IUnwalkableTileActionsHandler
{
    private readonly IMapActions _mapActions;
    private readonly IInGameDialogActions _inGameDialogActions;
    private readonly ICharacterActions _characterActions;

    public UnwalkableTileActionsHandler(IMapActions mapActions,
                                        IInGameDialogActions inGameDialogActions,
                                        ICharacterActions characterActions)
    {
        _mapActions = mapActions;
        _inGameDialogActions = inGameDialogActions;
        _characterActions = characterActions;
    }

    public void HandleUnwalkableTileActions(IReadOnlyList<UnwalkableTileAction> unwalkableActions, IMapCellState cellState)
    {
        foreach (var action in unwalkableActions)
        {
            switch (action)
            {
                case UnwalkableTileAction.Chest:
                    _mapActions.OpenChest(cellState.Coordinate);
                    _inGameDialogActions.ShowChestDialog();
                    break;
                case UnwalkableTileAction.Locker:
                    _mapActions.OpenLocker(cellState.Coordinate);
                    _inGameDialogActions.ShowLockerDialog();
                    break;
                case UnwalkableTileAction.Chair:
                    _characterActions.Sit(cellState.Coordinate, isChair: true);
                    break;
                case UnwalkableTileAction.Door:
                    cellState.Warp.MatchSome(w => _mapActions.OpenDoor(w));
                    break;
                case UnwalkableTileAction.Board:
                    _mapActions.OpenBoard(cellState.TileSpec);
                    _inGameDialogActions.ShowBoardDialog();
                    break;
                case UnwalkableTileAction.Jukebox:
                    _mapActions.OpenJukebox(cellState.Coordinate);
                    _inGameDialogActions.ShowJukeboxDialog();
                    break;
            }
        }
    }
}

public interface IUnwalkableTileActionsHandler
{
    void HandleUnwalkableTileActions(IReadOnlyList<UnwalkableTileAction> unwalkableActions, IMapCellState cellState);
}