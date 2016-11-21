// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;

namespace EOLib.Domain.Character
{
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
            var mainCharacter = _characterProvider.MainCharacter;
            var renderProperties = mainCharacter.RenderProperties;
            var destX = renderProperties.GetDestinationX();
            var destY = renderProperties.GetDestinationY();

            var cellState = _mapCellStateProvider.GetCellStateAt(destX, destY);

            if (cellState.Character.HasValue) //todo: walk through players after certain elapsed time
                return mainCharacter.NoWall && IsTileSpecWalkable(cellState.TileSpec);
            if (cellState.NPC.HasValue)
                return mainCharacter.NoWall && IsTileSpecWalkable(cellState.TileSpec);
            if (cellState.Warp.HasValue)
                return mainCharacter.NoWall || IsWarpWalkable(cellState.Warp.Value, cellState.TileSpec);

            return mainCharacter.NoWall || IsTileSpecWalkable(cellState.TileSpec);
        }

        private bool IsWarpWalkable(IWarp warp, TileSpec tile)
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
                    throw new ArgumentOutOfRangeException("tileSpec", tileSpec, null);
            }
        }
    }

    public interface IWalkValidationActions
    {
        bool CanMoveToDestinationCoordinates();
    }
}
