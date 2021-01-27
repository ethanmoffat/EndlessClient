using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

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
        private readonly List<DoorTimePair> _cachedDoorState;

        public DynamicMapObjectUpdater(ICharacterProvider characterProvider,
                                       ICurrentMapStateRepository currentMapStateRepository)
        {
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;

            _cachedDoorState = new List<DoorTimePair>();
        }

        public void UpdateMapObjects(GameTime gameTime)
        {
            var now = DateTime.Now;
            OpenNewDoors(now);
            CloseExpiredDoors(now);

            RemoveStaleSpikeTraps();
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
            _currentMapStateRepository.VisibleSpikeTraps.RemoveWhere(
                spike => _currentMapStateRepository.Characters
                                                   .Concat(new[] { _characterProvider.MainCharacter })
                                                   .Select(x => x.RenderProperties)
                                                   .All(x => x.MapX != spike.X && x.MapY != spike.Y));

        }
    }

    public interface IDynamicMapObjectUpdater
    {
        void UpdateMapObjects(GameTime gameTime);
    }
}
