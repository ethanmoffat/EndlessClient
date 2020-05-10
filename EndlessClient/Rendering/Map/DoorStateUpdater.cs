using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Map
{
    [MappedType(BaseType = typeof(IDoorStateUpdater), IsSingleton = true)]
    public class DoorStateUpdater : IDoorStateUpdater
    {
        private const int DOOR_CLOSE_TIME_MS = 3000;

        private class DoorTimePair
        {
            public IWarp Door { get; set; }
            public DateTime OpenTime { get; set; }
        }

        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly List<DoorTimePair> _cachedDoorState;

        public DoorStateUpdater(ICurrentMapStateRepository currentMapStateRepository)
        {
            _currentMapStateRepository = currentMapStateRepository;

            _cachedDoorState = new List<DoorTimePair>();
        }

        public void UpdateDoorState(GameTime gameTime)
        {
            var now = DateTime.Now;
            OpenNewDoors(now);
            CloseExpiredDoors(now);
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
    }

    public interface IDoorStateUpdater
    {
        void UpdateDoorState(GameTime gameTime);
    }
}
