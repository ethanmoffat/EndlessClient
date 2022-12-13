using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Stopwatch _ghostAttemptTime = new Stopwatch();
        private Character _characterToGhost = null;
        private const double GHOST_TIME = 2.0d;

        public GhostingRepository(ICharacterProvider characterProvider)
        {
            _characterProvider = characterProvider;
        }

        public bool GhostedRecently() => _ghostAttemptTime.Elapsed.Seconds > 0 && !_ghostAttemptTime.IsRunning;

        public bool CanGhostPlayer(Character c)
        {
            void _resetGhosting()
            {
                _ghostAttemptTime.Reset();
                _ghostAttemptTime.Start();
                _characterToGhost = c;
            }

            if (c != _characterToGhost) _resetGhosting();

            if (_ghostAttemptTime.Elapsed.TotalSeconds > GHOST_TIME && _characterToGhost == c)
            {
                _ghostAttemptTime.Stop();
                return true;
            }

            if (GhostedRecently()) _resetGhosting();

            return false;
        }

        public void ClearCacheIfNeeded()
        {
            if (_characterToGhost == null)
                return;

            var mc = _characterProvider.MainCharacter;
            var playersDiff = Math.Abs(mc.RenderProperties.MapX - _characterToGhost.RenderProperties.MapX) +
                Math.Abs(mc.RenderProperties.MapY - _characterToGhost.RenderProperties.MapY);

            var playersAreTooFar = playersDiff > 2;
            var playersAreOnTopOfEachother = playersDiff == 0;
            var timerHasBeenTooLong = _ghostAttemptTime.Elapsed.TotalSeconds > GHOST_TIME + 1;

            if (playersAreTooFar || playersAreOnTopOfEachother || timerHasBeenTooLong)
                ResetState();
        }

        public void ResetState()
        {
            _ghostAttemptTime.Stop();
            _ghostAttemptTime.Reset();
            _characterToGhost = null;
        }
    }
}

