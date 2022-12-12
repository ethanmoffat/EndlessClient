using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using Optional;
using System;
using System.Linq;

namespace EOLib.Domain.Character
{
    public enum WalkValidationResult
    {
        BlockedByCharacter,
        GhostComplete,
        NotWalkable,
        Walkable
    }

    [AutoMappedType]
    public class WalkValidationActions : IWalkValidationActions
    {
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IUnlockDoorValidator _unlockDoorValidator;

        private DateTime? _startedGhosting = null;
        private Character _currentlyGhosting = null;
        private DateTime? _finishedGhosting = null;

        public WalkValidationActions(IMapCellStateProvider mapCellStateProvider,
                                     ICharacterProvider characterProvider,
                                     ICurrentMapStateProvider currentMapStateProvider,
                                     IUnlockDoorValidator unlockDoorValidator)
        {
            _mapCellStateProvider = mapCellStateProvider;
            _characterProvider = characterProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _unlockDoorValidator = unlockDoorValidator;
        }

        public void ClearGhostingCacheIfNeeded()
        {
            if (_currentlyGhosting == null)
                return;

            var mc = _characterProvider.MainCharacter;
            var playersXDiff = Math.Abs(mc.RenderProperties.MapX - _currentlyGhosting.RenderProperties.MapX);
            var playersYDiff = Math.Abs(mc.RenderProperties.MapY - _currentlyGhosting.RenderProperties.MapY);
            var playersAreNextToEachother =  playersXDiff == 1 || playersYDiff == 1;
            var playersAreOnTopOfEachother = playersXDiff == 0 && playersYDiff == 0;

            var timerHasBeenTooLong = _finishedGhosting.HasValue ? (DateTime.Now - _finishedGhosting.Value).TotalSeconds > 2 : false;

            if (!playersAreNextToEachother || playersAreOnTopOfEachother || timerHasBeenTooLong)
            {
                _startedGhosting = null;
                _currentlyGhosting = null;
                _finishedGhosting = null;
            }
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
            var mc = _characterProvider.MainCharacter;

            var cellChar = cellState.Character.FlatMap(c => c.SomeWhen(cc => cc != mc));

            return cellChar.Match(
                some: c => {
                    if (!CanGhostOtherCharacter(c)) return WalkValidationResult.BlockedByCharacter;
                    return (mc.NoWall || IsTileSpecWalkable(cellState.TileSpec)) ? _finishedGhosting != null ? WalkValidationResult.GhostComplete : WalkValidationResult.Walkable : WalkValidationResult.NotWalkable;
                },
                none: () => cellState.NPC.Match(
                    some: _ => (mc.NoWall && IsTileSpecWalkable(cellState.TileSpec)) ? WalkValidationResult.Walkable : WalkValidationResult.NotWalkable,
                    none: () => cellState.Warp.Match(
                        some: w => (mc.NoWall || IsWarpWalkable(w, cellState.TileSpec)) ? WalkValidationResult.Walkable : WalkValidationResult.NotWalkable,
                        none: () => (mc.NoWall || IsTileSpecWalkable(cellState.TileSpec)) ? WalkValidationResult.Walkable : WalkValidationResult.NotWalkable)));
        }

        private bool CanGhostOtherCharacter(Character c)
        {
            void _resetGhosting()
            {
                _finishedGhosting = null;
                _startedGhosting = DateTime.Now;
                _currentlyGhosting = c;
            }

            if (c != _currentlyGhosting) _resetGhosting();

            if (_startedGhosting.HasValue && (DateTime.Now - _startedGhosting.Value).TotalSeconds > 3 && _currentlyGhosting == c)
            {
                _finishedGhosting = DateTime.Now;
                return true;
            }

            if (!_startedGhosting.HasValue) _resetGhosting();

            return false;
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
    }

    public interface IWalkValidationActions
    {
        void ClearGhostingCacheIfNeeded();

        WalkValidationResult CanMoveToDestinationCoordinates();

        WalkValidationResult CanMoveToCoordinates(int gridX, int gridY);

        WalkValidationResult IsCellStateWalkable(IMapCellState cellState);
    }
}
