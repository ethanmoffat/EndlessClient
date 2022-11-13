using AutomaticTypeMapper;
using System.Diagnostics;

namespace EndlessClient.Rendering
{
    [AutoMappedType(IsSingleton = true)]
    public class FixedTimeStepRepository : IFixedTimeStepRepository
    {
        private const int FIXED_UPDATE_TIME_MS = 24; // 40 FPS (walk updates at 10 FPS)

        private int _isWalkUpdate;

        public Stopwatch FixedUpdateTimer { get; set; }

        public bool IsUpdateFrame => FixedUpdateTimer.ElapsedMilliseconds > FIXED_UPDATE_TIME_MS;

        public bool IsWalkUpdateFrame => IsUpdateFrame && _isWalkUpdate == 3;

        public FixedTimeStepRepository() => FixedUpdateTimer = Stopwatch.StartNew();

        public void RestartTimer()
        {
            FixedUpdateTimer.Restart();
            _isWalkUpdate = ++_isWalkUpdate % 4;
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
