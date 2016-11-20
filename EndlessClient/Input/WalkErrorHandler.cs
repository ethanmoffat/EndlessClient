// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.HUD;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using EOLib.Localization;

namespace EndlessClient.Input
{
    public class WalkErrorHandler : IWalkErrorHandler
    {
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;

        public WalkErrorHandler(IMapCellStateProvider mapCellStateProvider,
                                ICharacterProvider characterProvider,
                                IStatusLabelSetter statusLabelSetter)
        {
            _mapCellStateProvider = mapCellStateProvider;
            _characterProvider = characterProvider;
            _statusLabelSetter = statusLabelSetter;
        }

        public void HandleWalkError()
        {
            var destX = MainCharacter.RenderProperties.GetDestinationX();
            var destY = MainCharacter.RenderProperties.GetDestinationY();

            var cellState = _mapCellStateProvider.GetCellStateAt(destX, destY);
            if (cellState.Character.HasValue) //todo: walk through players after certain elapsed time (3-5sec?)
                HandleWalkThroughOtherCharacter();
            else if (cellState.Warp.HasValue)
                HandleWalkToWarpTile(cellState.Warp.Value);
            else
                HandleWalkToTileSpec(cellState);
        }

        private void HandleWalkThroughOtherCharacter()
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
            if (warp.DoorType != DoorSpec.Door)
            {
                //todo: handle doors
                //    DoorSpec doorOpened;
                //    if (!warpInfo.IsDoorOpened && !warpInfo.DoorPacketSent)
                //    {
                //        if ((doorOpened = Character.CanOpenDoor(warpInfo.DoorType)) == DoorSpec.Door)
                //            mapRend.StartOpenDoor(warpInfo, destX, destY);
                //    }
                //    else
                //    {
                //        //normal walking
                //        if ((doorOpened = Character.CanOpenDoor(warpInfo.DoorType)) == DoorSpec.Door)
                //            _walkIfValid(TileSpec.None, direction, destX, destY);
                //    }

                //    if (doorOpened != DoorSpec.Door)
                //    {
                //        string strWhichKey = "[error key?]";
                //        switch (doorOpened)
                //        {
                //            case DoorSpec.LockedCrystal:
                //                strWhichKey = "Crystal Key";
                //                break;
                //            case DoorSpec.LockedSilver:
                //                strWhichKey = "Silver Key";
                //                break;
                //            case DoorSpec.LockedWraith:
                //                strWhichKey = "Wraith Key";
                //                break;
                //        }

                //        EOMessageBox.Show(DialogResourceID.DOOR_LOCKED, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                //        ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                //            EOResourceID.STATUS_LABEL_THE_DOOR_IS_LOCKED_EXCLAMATION,
                //            " - " + strWhichKey);
                //    }
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
                case TileSpec.ChairDown: //todo: chairs
                case TileSpec.ChairLeft:
                case TileSpec.ChairRight:
                case TileSpec.ChairUp:
                case TileSpec.ChairDownRight:
                case TileSpec.ChairUpLeft:
                case TileSpec.ChairAll:
                    break;
                case TileSpec.Chest: //todo: chests
                    //if (!walkValid)
                    //{
                    //    var chest = OldWorld.Instance.ActiveMapRenderer.MapRef.Chests.Single(_c => _c.X == destX && _c.Y == destY);
                    //    if (chest != null)
                    //    {
                    //        string requiredKey = null;
                    //        switch (Character.CanOpenChest(chest))
                    //        {
                    //            case ChestKey.Normal: requiredKey = "Normal Key"; break;
                    //            case ChestKey.Silver: requiredKey = "Silver Key"; break;
                    //            case ChestKey.Crystal: requiredKey = "Crystal Key"; break;
                    //            case ChestKey.Wraith: requiredKey = "Wraith Key"; break;
                    //            default:
                    //                ChestDialog.Show(((EOGame)Game).API, (byte)chest.X, (byte)chest.Y);
                    //                break;
                    //        }

                    //        if (requiredKey != null)
                    //        {
                    //            EOMessageBox.Show(DialogResourceID.CHEST_LOCKED, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    //            ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_THE_CHEST_IS_LOCKED_EXCLAMATION,
                    //                " - " + requiredKey);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        ChestDialog.Show(((EOGame)Game).API, destX, destY);
                    //    }
                    //}
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

        private ICharacter MainCharacter { get { return _characterProvider.MainCharacter; } }
    }

    public interface IWalkErrorHandler
    {
        void HandleWalkError();
    }
}
