using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Input;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Rendering.Map
{
    [AutoMappedType(IsSingleton = true)]
    public class DynamicMapObjectUpdater : IDynamicMapObjectUpdater
    {
        private const int DOOR_CLOSE_TIME_MS = 3000;

        private class DoorTimePair
        {
            public IWarp Door { get; set; }
            public DateTime OpenTime { get; set; }
        }

        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IUserInputProvider _userInputProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMapObjectBoundsCalculator _mapObjectBoundsCalculator;
        private readonly IMapInteractionController _mapInteractionController;
        private readonly List<DoorTimePair> _cachedDoorState;

        public DynamicMapObjectUpdater(ICharacterProvider characterProvider,
                                       ICurrentMapStateRepository currentMapStateRepository,
                                       IUserInputProvider userInputProvider,
                                       ICurrentMapProvider currentMapProvider,
                                       IMapObjectBoundsCalculator mapObjectBoundsCalculator,
                                       IMapInteractionController mapInteractionController)
        {
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _userInputProvider = userInputProvider;
            _currentMapProvider = currentMapProvider;
            _mapObjectBoundsCalculator = mapObjectBoundsCalculator;
            _mapInteractionController = mapInteractionController;
            _cachedDoorState = new List<DoorTimePair>();
        }

        public void UpdateMapObjects(GameTime gameTime)
        {
            var now = DateTime.Now;
            OpenNewDoors(now);
            CloseExpiredDoors(now);

            RemoveStaleSpikeTraps();

            CheckForObjectClicks();
        }

        private void OpenNewDoors(DateTime now)
        {
            var newDoors = _currentMapStateRepository.OpenDoors.Where(x => _cachedDoorState.All(d => d.Door != x));
            foreach (var door in newDoors)
                _cachedDoorState.Add(new DoorTimePair { Door = door, OpenTime = now });
        }

        private void CloseExpiredDoors(DateTime now)
        {
            var expiredDoors = _cachedDoorState.Where(x => (now - x.OpenTime).TotalMilliseconds > DOOR_CLOSE_TIME_MS).ToList();
            foreach (var door in expiredDoors)
            {
                _cachedDoorState.Remove(door);
                _currentMapStateRepository.OpenDoors.Remove(door.Door);
            }
        }

        private void RemoveStaleSpikeTraps()
        {
            var staleTraps = new List<MapCoordinate>();

            foreach (var spikeTrap in _currentMapStateRepository.VisibleSpikeTraps)
            {
                if (_currentMapStateRepository.Characters.Values
                    .Concat(new[] { _characterProvider.MainCharacter })
                    .Select(x => x.RenderProperties)
                    .All(x => x.MapX != spikeTrap.X && x.MapY != spikeTrap.Y))
                {
                    staleTraps.Add(spikeTrap);
                }
            }

            _currentMapStateRepository.VisibleSpikeTraps.RemoveWhere(staleTraps.Contains);
        }

        private void CheckForObjectClicks()
        {
            var mouseClicked = _userInputProvider.PreviousMouseState.LeftButton == ButtonState.Pressed &&
                _userInputProvider.CurrentMouseState.LeftButton == ButtonState.Released;

            if (mouseClicked)
            {
                foreach (var sign in _currentMapProvider.CurrentMap.Signs)
                {
                    var gfx = _currentMapProvider.CurrentMap.GFX[MapLayer.Objects][sign.Y, sign.X];
                    if (gfx > 0)
                    {
                        var bounds = _mapObjectBoundsCalculator.GetMapObjectBounds(sign.X, sign.Y, gfx);
                        if (bounds.Contains(_userInputProvider.CurrentMouseState.Position))
                        {
                            var cellState = new MapCellState
                            {
                                Sign = Option.Some<ISign>(new Sign(sign)),
                                Coordinate = new MapCoordinate(sign.X, sign.Y)
                            };
                            _mapInteractionController.LeftClick(cellState, Option.None<IMouseCursorRenderer>());
                            break;
                        }
                    }
                }

                // todo: check for board object clicks
            }
        }
    }

    public interface IDynamicMapObjectUpdater
    {
        void UpdateMapObjects(GameTime gameTime);
    }
}
