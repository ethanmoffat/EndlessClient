using AutomaticTypeMapper;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Communication;
using Optional;

namespace EndlessClient.Input
{
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
        private readonly IPacketSendService _packetSendService;
        private readonly IEOMessageBoxFactory _messageBoxFactory;

        public UnwalkableTileActions(IMapCellStateProvider mapCellStateProvider,
                                     ICharacterProvider characterProvider,
                                     IStatusLabelSetter statusLabelSetter,
                                     ICurrentMapStateRepository currentMapStateRepository,
                                     IUnlockDoorValidator unlockDoorValidator,
                                     IUnlockChestValidator unlockChestValidator,
                                     IEOMessageBoxFactory eoMessageBoxFactory,
                                     IPacketSendService packetSendService,
                                     IEOMessageBoxFactory messageBoxFactory)
        {
            _mapCellStateProvider = mapCellStateProvider;
            _characterProvider = characterProvider;
            _statusLabelSetter = statusLabelSetter;
            _currentMapStateRepository = currentMapStateRepository;
            _unlockDoorValidator = unlockDoorValidator;
            _unlockChestValidator = unlockChestValidator;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _packetSendService = packetSendService;
            _messageBoxFactory = messageBoxFactory;
        }

        public void HandleUnwalkableTile()
        {
            if (MainCharacter.RenderProperties.SitState != SitState.Standing)
                return;

            var destX = MainCharacter.RenderProperties.GetDestinationX();
            var destY = MainCharacter.RenderProperties.GetDestinationY();

            var cellState = _mapCellStateProvider.GetCellStateAt(destX, destY);
            cellState.Character.Match(
                some: c => HandleWalkThroughOtherCharacter(c), //todo: walk through players after certain elapsed time (3-5sec?)
                none: () => cellState.Warp.Match(
                    some: w => HandleWalkToWarpTile(w),
                    none: () => HandleWalkToTileSpec(cellState)));
        }

        private void HandleWalkThroughOtherCharacter(ICharacter c)
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
        }

        private void HandleWalkToWarpTile(IWarp warp)
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
                    var packet = new PacketBuilder(PacketFamily.Door, PacketAction.Open)
                        .AddChar((byte) warp.X)
                        .AddChar((byte) warp.Y)
                        .Build();

                    _packetSendService.SendPacket(packet);
                    _currentMapStateRepository.PendingDoors.Add(warp);
                }
            }
            else if (warp.LevelRequirement > 0 && MainCharacter.Stats[CharacterStat.Level] < warp.LevelRequirement)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                    EOResourceID.STATUS_LABEL_NOT_READY_TO_USE_ENTRANCE,
                    " - LVL " + warp.LevelRequirement);
            }
        }

        private void HandleWalkToTileSpec(IMapCellState cellState)
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
                    HandleWalkToChair();
                    break;
                case TileSpec.Chest:
                    cellState.ChestKey.Match(
                        some: key =>
                        {
                            if (!_unlockChestValidator.CanMainCharacterOpenChest(key))
                            {
                                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.CHEST_LOCKED);
                                dlg.ShowDialog();

                                var requiredKey = _unlockChestValidator.GetRequiredKeyName(key);
                                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_THE_CHEST_IS_LOCKED_EXCLAMATION, " - " + requiredKey);
                            }
                            else
                            {
                                // todo: chest request, show dialog
                            }
                        },
                        none: () =>
                        {
                            // todo: chest request, show dialog
                        });
                    break;
                case TileSpec.BankVault: //todo: locker
                    //walkValid = Renderer.NoWall;
                    //if (!walkValid)
                    //{
                    //    LockerDialog.Show(((EOGame)Game).API, destX, destY);
                    //}
                    break;
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
        }

        private void HandleWalkToChair()
        {
            // server validates that chair direction is OK and that no one is already sitting there
            var rp = _characterProvider.MainCharacter.RenderProperties;
            var action = rp.SitState == SitState.Chair ? SitAction.Stand : SitAction.Sit;
            var packet = new PacketBuilder(PacketFamily.Chair, PacketAction.Request)
                .AddChar((byte)action)
                .AddChar((byte)rp.GetDestinationX())
                .AddChar((byte)rp.GetDestinationY())
                .Build();

            _packetSendService.SendPacket(packet);
        }

        private ICharacter MainCharacter => _characterProvider.MainCharacter;
    }

    public interface IUnwalkableTileActions
    {
        void HandleUnwalkableTile();
    }
}
