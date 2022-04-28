using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using EOLib.Localization;

namespace EndlessClient.Input
{
    public enum UnwalkableTileAction
    {
        None,
        Chair,
        Chest,
        Door,
        Locker,
    }

    [AutoMappedType]
    public class UnwalkableTileActions : IUnwalkableTileActions
    {
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IUnlockDoorValidator _unlockDoorValidator;
        private readonly IUnlockChestValidator _unlockChestValidator;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IEOMessageBoxFactory _messageBoxFactory;

        public UnwalkableTileActions(IMapCellStateProvider mapCellStateProvider,
                                     ICharacterProvider characterProvider,
                                     IStatusLabelSetter statusLabelSetter,
                                     ICurrentMapStateRepository currentMapStateRepository,
                                     IUnlockDoorValidator unlockDoorValidator,
                                     IUnlockChestValidator unlockChestValidator,
                                     IEOMessageBoxFactory eoMessageBoxFactory,
                                     IEOMessageBoxFactory messageBoxFactory)
        {
            _mapCellStateProvider = mapCellStateProvider;
            _characterProvider = characterProvider;
            _statusLabelSetter = statusLabelSetter;
            _currentMapStateRepository = currentMapStateRepository;
            _unlockDoorValidator = unlockDoorValidator;
            _unlockChestValidator = unlockChestValidator;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _messageBoxFactory = messageBoxFactory;
        }

        public (UnwalkableTileAction, IMapCellState) HandleUnwalkableTile()
        {
            var destX = MainCharacter.RenderProperties.GetDestinationX();
            var destY = MainCharacter.RenderProperties.GetDestinationY();
            return HandleUnwalkableTile(destX, destY);
        }

        public (UnwalkableTileAction, IMapCellState) HandleUnwalkableTile(int x, int y)
        {
            var cellState = _mapCellStateProvider.GetCellStateAt(x, y);
            var action = HandleUnwalkableTile(cellState);
            return (action, cellState);
        }

        public UnwalkableTileAction HandleUnwalkableTile(IMapCellState cellState)
        {
            if (MainCharacter.RenderProperties.SitState != SitState.Standing)
                return UnwalkableTileAction.None;

            return cellState.Character.Match(
                some: c => HandleWalkThroughOtherCharacter(c), //todo: walk through players after certain elapsed time (3-5sec?)
                none: () => cellState.Warp.Match(
                    some: w => HandleWalkToWarpTile(w),
                    none: () => HandleWalkToTileSpec(cellState)));
        }

        private UnwalkableTileAction HandleWalkThroughOtherCharacter(Character c)
        {
            //        EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
            //            EOResourceID.STATUS_LABEL_KEEP_MOVING_THROUGH_PLAYER);
            //        if (_startWalkingThroughPlayerTime == null)
            //            _startWalkingThroughPlayerTime = DateTime.Now;
            //        else if ((DateTime.Now - _startWalkingThroughPlayerTime.Value).TotalSeconds > 5)
            //        {
            //            _startWalkingThroughPlayerTime = null;
            //            goto case TileInfoReturnType.IsTileSpec;
            //        }
            return UnwalkableTileAction.None;
        }

        private UnwalkableTileAction HandleWalkToWarpTile(Warp warp)
        {
            if (warp.DoorType != DoorSpec.NoDoor)
            {
                if (!_unlockDoorValidator.CanMainCharacterOpenDoor(warp))
                {
                    var requiredKey = _unlockDoorValidator.GetRequiredKey(warp);

                    var messageBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.DOOR_LOCKED);
                    messageBox.ShowDialog();
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                        EOResourceID.STATUS_LABEL_THE_DOOR_IS_LOCKED_EXCLAMATION,
                        " - " + requiredKey);
                }
                else if (!_currentMapStateRepository.OpenDoors.Contains(warp) &&
                         !_currentMapStateRepository.PendingDoors.Contains(warp))
                {
                    return UnwalkableTileAction.Door;
                }
            }
            else if (warp.LevelRequirement > 0 && MainCharacter.Stats[CharacterStat.Level] < warp.LevelRequirement)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                    EOResourceID.STATUS_LABEL_NOT_READY_TO_USE_ENTRANCE,
                    " - LVL " + warp.LevelRequirement);
            }

            return UnwalkableTileAction.None;
        }

        private UnwalkableTileAction HandleWalkToTileSpec(IMapCellState cellState)
        {
            switch (cellState.TileSpec)
            {
                case TileSpec.ChairDown:
                case TileSpec.ChairLeft:
                case TileSpec.ChairRight:
                case TileSpec.ChairUp:
                case TileSpec.ChairDownRight:
                case TileSpec.ChairUpLeft:
                case TileSpec.ChairAll:
                    return UnwalkableTileAction.Chair;
                case TileSpec.Chest:
                    return cellState.ChestKey.Match(
                        some: key =>
                        {
                            if (!_unlockChestValidator.CanMainCharacterOpenChest(key))
                            {
                                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.CHEST_LOCKED);
                                dlg.ShowDialog();

                                var requiredKey = _unlockChestValidator.GetRequiredKeyName(key);
                                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_THE_CHEST_IS_LOCKED_EXCLAMATION, requiredKey.Match(x => $" - {x}", () => string.Empty));
                                return UnwalkableTileAction.None;
                            }
                            else
                            {
                                return UnwalkableTileAction.Chest;
                            }
                        },
                        none: () =>
                        {
                            return UnwalkableTileAction.Chest;
                        });
                case TileSpec.BankVault:
                    return UnwalkableTileAction.Locker;
                case TileSpec.Board1: //todo: boards
                case TileSpec.Board2:
                case TileSpec.Board3:
                case TileSpec.Board4:
                case TileSpec.Board5:
                case TileSpec.Board6:
                case TileSpec.Board7:
                case TileSpec.Board8:
                    break;
                case TileSpec.Jukebox: //todo: jukebox
                    break;
            }

            return UnwalkableTileAction.None;
        }

        private Character MainCharacter => _characterProvider.MainCharacter;
    }

    public interface IUnwalkableTileActions
    {
        (UnwalkableTileAction, IMapCellState) HandleUnwalkableTile();

        (UnwalkableTileAction, IMapCellState) HandleUnwalkableTile(int x, int y);

        UnwalkableTileAction HandleUnwalkableTile(IMapCellState mapCellState);
    }
}
