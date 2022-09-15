using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using Optional;
using System;
using System.Linq;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
    public class WalkValidationActions : IWalkValidationActions
    {
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IUnlockDoorValidator _unlockDoorValidator;

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

        public bool CanMoveToDestinationCoordinates()
        {
            if (_currentMapStateProvider.MapWarpState == WarpState.WarpStarted)
                return false;

            var renderProperties = _characterProvider.MainCharacter.RenderProperties;
            var destX = renderProperties.GetDestinationX();
            var destY = renderProperties.GetDestinationY();

            return CanMoveToCoordinates(destX, destY);
        }

        public bool CanMoveToCoordinates(int gridX, int gridY)
        {
            var mainCharacter = _characterProvider.MainCharacter;

            if (mainCharacter.RenderProperties.SitState != SitState.Standing)
                return false;

            var cellState = _mapCellStateProvider.GetCellStateAt(gridX, gridY);
            return IsCellStateWalkable(cellState);
        }

        public bool IsCellStateWalkable(IMapCellState cellState)
        {
            var mc = _characterProvider.MainCharacter;

            var cellChar = cellState.Character.FlatMap(c => c.SomeWhen(cc => cc != mc));

            return cellChar.Match(
                some: _ => mc.NoWall && IsTileSpecWalkable(cellState.TileSpec),
                none: () => cellState.NPC.Match(
                    some: _ => mc.NoWall && IsTileSpecWalkable(cellState.TileSpec),
                    none: () => cellState.Warp.Match(
                        some: w => mc.NoWall || IsWarpWalkable(w, cellState.TileSpec),
                        none: () => mc.NoWall || IsTileSpecWalkable(cellState.TileSpec))));
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

        private static bool IsTileSpecWalkable(TileSpec tileSpec)
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
                    return false;
            }
        }
    }

    public interface IWalkValidationActions
    {
        bool CanMoveToDestinationCoordinates();

        bool CanMoveToCoordinates(int gridX, int gridY);

        bool IsCellStateWalkable(IMapCellState cellState);
    }
}
