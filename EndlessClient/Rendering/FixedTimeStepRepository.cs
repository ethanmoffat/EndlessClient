using AutomaticTypeMapper;
using System.Diagnostics;

namespace EndlessClient.Rendering
{
    [AutoMappedType(IsSingleton = true)]
    public class FixedTimeStepRepository : IFixedTimeStepRepository
    {
        private const int FIXED_UPDATE_TIME_MS = 51; // ~19 FPS

        private bool _isWalkUpdate;

        public Stopwatch FixedUpdateTimer { get; set; }

        public bool IsUpdateFrame => FixedUpdateTimer.ElapsedMilliseconds > FIXED_UPDATE_TIME_MS;

        public bool IsWalkUpdateFrame => IsUpdateFrame && _isWalkUpdate;

        public FixedTimeStepRepository() => FixedUpdateTimer = Stopwatch.StartNew();

        public void RestartTimer()
        {
            FixedUpdateTimer.Restart();
            _isWalkUpdate = !_isWalkUpdate;
        }
    }

    public interface IFixedTimeStepRepository
    {
        Stopwatch FixedUpdateTimer { get; set; }

        bool IsUpdateFrame { get; }

        bool IsWalkUpdateFrame { get; }

        void RestartTimer();
    }
}
