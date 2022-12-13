using System;
using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{

    public interface IGhostingRepository : IResettable
    {
        bool CanGhostPlayer(Character c);
        bool GhostedRecently();
        void ClearCacheIfNeeded();
    }

    public interface IGhostingProvider : IResettable
    {
        bool CanGhostPlayer(Character c);
        bool GhostedRecently();
        void ClearCacheIfNeeded();
    }

    [AutoMappedType(IsSingleton = true)]
    public sealed class GhostingRepository: IGhostingProvider, IGhostingRepository
    {
        private readonly ICharacterProvider _characterProvider;

        private DateTime? _startedGhosting = null;
        private Character _currentlyGhosting = null;
        private DateTime? _finishedGhosting = null;
        private const double GHOST_TIME = 2.0d;

        public GhostingRepository(ICharacterProvider characterProvider)
        {
            _characterProvider = characterProvider;
        }

        public bool GhostedRecently() => _finishedGhosting != null;

        public bool CanGhostPlayer(Character c)
        {
            void _resetGhosting()
            {
                _finishedGhosting = null;
                _startedGhosting = DateTime.Now;
                _currentlyGhosting = c;
            }

            if (c != _currentlyGhosting) _resetGhosting();

            if (_startedGhosting.HasValue && (DateTime.Now - _startedGhosting.Value).TotalSeconds > GHOST_TIME + 1 && _currentlyGhosting == c)
            {
                _finishedGhosting = DateTime.Now;
                return true;
            }

            if (!_startedGhosting.HasValue) _resetGhosting();

            return false;
        }

        public void ClearCacheIfNeeded()
        {
            if (_currentlyGhosting == null)
                return;

            var mc = _characterProvider.MainCharacter;
            var playersXDiff = Math.Abs(mc.RenderProperties.MapX - _currentlyGhosting.RenderProperties.MapX);
            var playersYDiff = Math.Abs(mc.RenderProperties.MapY - _currentlyGhosting.RenderProperties.MapY);
            var playersAreNextToEachother = playersXDiff == 1 || playersYDiff == 1;
            var playersAreOnTopOfEachother = playersXDiff == 0 && playersYDiff == 0;

            var timerHasBeenTooLong = _finishedGhosting.HasValue && (DateTime.Now - _finishedGhosting.Value).TotalSeconds > GHOST_TIME;

            if (!playersAreNextToEachother || playersAreOnTopOfEachother || timerHasBeenTooLong)
                ResetState();
        }

        public void ResetState()
        {
            _startedGhosting = null;
            _currentlyGhosting = null;
            _finishedGhosting = null;
        }
    }
}

