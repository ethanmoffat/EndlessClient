﻿using System;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using Optional;

namespace EOLib.Domain.Character
{
    public enum WalkValidationResult
    {
        NotWalkable,
        Walkable,
        BlockedByCharacter,
        GhostComplete
    }

    [AutoMappedType]
    public class WalkValidationActions : IWalkValidationActions
    {
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IUnlockDoorValidator _unlockDoorValidator;
        private readonly IGhostingRepository _ghostingRepository;

        public WalkValidationActions(IMapCellStateProvider mapCellStateProvider,
                                     ICharacterProvider characterProvider,
                                     ICurrentMapStateProvider currentMapStateProvider,
                                     IUnlockDoorValidator unlockDoorValidator,
                                     IGhostingRepository ghostingRepository)
        {
            _mapCellStateProvider = mapCellStateProvider;
            _characterProvider = characterProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _unlockDoorValidator = unlockDoorValidator;
            _ghostingRepository = ghostingRepository;
        }

        public WalkValidationResult CanMoveToDestinationCoordinates()
        {
            if (_currentMapStateProvider.MapWarpState == WarpState.WarpStarted)
                return WalkValidationResult.NotWalkable;

            var renderProperties = _characterProvider.MainCharacter.RenderProperties;
            var destX = renderProperties.GetDestinationX();
            var destY = renderProperties.GetDestinationY();

            return CanMoveToCoordinates(destX, destY);
        }

        public WalkValidationResult CanMoveToCoordinates(int gridX, int gridY)
        {
            var mainCharacter = _characterProvider.MainCharacter;

            if (mainCharacter.RenderProperties.SitState != SitState.Standing)
                return WalkValidationResult.NotWalkable;

            var cellState = _mapCellStateProvider.GetCellStateAt(gridX, gridY);
            return IsCellStateWalkable(cellState);
        }

        public WalkValidationResult IsCellStateWalkable(IMapCellState cellState)
        {
            ClearGhostCache();

            var mc = _characterProvider.MainCharacter;

            var cellChar = cellState.Character.FlatMap(c => c.SomeWhen(cc => cc != mc));

            return cellChar.Match(
                some: c =>
                {
                    if (mc.NoWall)
                        return WalkValidationResult.Walkable;

                    if (!CanGhostPlayer(c))
                        return WalkValidationResult.BlockedByCharacter;

                    if (IsTileSpecWalkable(cellState.TileSpec) && _ghostingRepository.GhostedRecently)
                        return WalkValidationResult.GhostComplete;

                    return BoolToWalkResult(IsTileSpecWalkable(cellState.TileSpec));
                },
                none: () => cellState.NPC.Match(
                    some: _ => BoolToWalkResult(mc.NoWall && IsTileSpecWalkable(cellState.TileSpec)),
                    none: () => cellState.Warp.Match(
                        some: w => BoolToWalkResult(mc.NoWall || IsWarpWalkable(w, cellState.TileSpec)),
                        none: () => BoolToWalkResult(mc.NoWall || IsTileSpecWalkable(cellState.TileSpec)))));
        }

        private bool IsWarpWalkable(Warp warp, TileSpec tile)
        {
            if (warp.DoorType != DoorSpec.NoDoor)
                return _currentMapStateProvider.OpenDoors.Any(w => w.X == warp.X && w.Y == warp.Y) &&
                       _unlockDoorValidator.CanMainCharacterOpenDoor(warp);
            if (warp.LevelRequirement != 0)
                return warp.LevelRequirement <= _characterProvider.MainCharacter.Stats[CharacterStat.Level];
            return IsTileSpecWalkable(tile);
        }

        private WalkValidationResult BoolToWalkResult(bool b) => b ? WalkValidationResult.Walkable : WalkValidationResult.NotWalkable;

        private bool IsTileSpecWalkable(TileSpec tileSpec)
        {
            switch (tileSpec)
            {
                case TileSpec.Wall:
                case TileSpec.ChairDown:
                case TileSpec.ChairLeft:
                case TileSpec.ChairRight:
                case TileSpec.ChairUp:
                case TileSpec.ChairDownRight:
                case TileSpec.ChairUpLeft:
                case TileSpec.ChairAll:
                case TileSpec.JammedDoor:
                case TileSpec.Chest:
                case TileSpec.BankVault:
                case TileSpec.MapEdge:
                case TileSpec.Board1:
                case TileSpec.Board2:
                case TileSpec.Board3:
                case TileSpec.Board4:
                case TileSpec.Board5:
                case TileSpec.Board6:
                case TileSpec.Board7:
                case TileSpec.Board8:
                case TileSpec.Jukebox:
                case TileSpec.VultTypo:
                    return false;
                case TileSpec.NPCBoundary:
                case TileSpec.FakeWall:
                case TileSpec.Jump:
                case TileSpec.Water:
                case TileSpec.Arena:
                case TileSpec.AmbientSource:
                case TileSpec.SpikesTimed:
                case TileSpec.SpikesStatic:
                case TileSpec.SpikesTrap:
                case TileSpec.None:
                    return true;
                default:
                    // These values were tested with the Vanilla 0.28 client
                    // TileSpec 10, 12, and 31 are walkable while other unknown values are not
                    return (int)tileSpec == 10 || (int)tileSpec == 12 || (int)tileSpec == 31;
            }
        }

        private void ClearGhostCache()
        {
            _ghostingRepository.GhostTarget.MatchSome(ghostTarget =>
            {
                var mc = _characterProvider.MainCharacter;
                var playersDiff = Math.Max(Math.Abs(mc.RenderProperties.MapX - ghostTarget.RenderProperties.MapX),
                    Math.Abs(mc.RenderProperties.MapY - ghostTarget.RenderProperties.MapY));

                var playersAreTooFar = playersDiff > 1;
                var playersAreOnTopOfEachother = playersDiff == 0;
                var timerHasBeenTooLong = _ghostingRepository.GhostStartTime.Elapsed.TotalSeconds > Constants.GhostTime + 1;

                if (playersAreTooFar || playersAreOnTopOfEachother || timerHasBeenTooLong)
                    _ghostingRepository.ResetState();
            });
        }

        private bool CanGhostPlayer(Character c)
        {
            void _setNewTarget(Character cc)
            {
                _ghostingRepository.GhostCompleted = false;
                _ghostingRepository.GhostStartTime.Reset();
                _ghostingRepository.GhostStartTime.Start();
                _ghostingRepository.GhostTarget = Option.Some(cc);
            }

            if (_ghostingRepository.GhostTarget.Match(x => x != c, () => true) || _ghostingRepository.GhostCompleted)
                _setNewTarget(c);

            if (_ghostingRepository.GhostStartTime.Elapsed.TotalSeconds > Constants.GhostTime &&
                _ghostingRepository.GhostTarget.Match(x => x == c, () => false))
            {
                _ghostingRepository.GhostStartTime.Stop();
                _ghostingRepository.GhostCompleted = true;
                return true;
            }

            if (_ghostingRepository.GhostedRecently)
                _setNewTarget(c);

            return false;
        }
    }

    public interface IWalkValidationActions
    {
        WalkValidationResult CanMoveToDestinationCoordinates();

        WalkValidationResult CanMoveToCoordinates(int gridX, int gridY);

        WalkValidationResult IsCellStateWalkable(IMapCellState cellState);
    }
}
