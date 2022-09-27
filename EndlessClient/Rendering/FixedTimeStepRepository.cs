using AutomaticTypeMapper;
using System.Diagnostics;

namespace EndlessClient.Rendering
{
    [AutoMappedType(IsSingleton = true)]
    public class FixedTimeStepRepository : IFixedTimeStepRepository
    {
        private const int FIXED_UPDATE_TIME_MS = 62;

        public Stopwatch FixedUpdateTimer { get; set; }

        public bool IsUpdateFrame => FixedUpdateTimer.ElapsedMilliseconds > FIXED_UPDATE_TIME_MS;

        public FixedTimeStepRepository() => FixedUpdateTimer = Stopwatch.StartNew();

        public void RestartTimer() => FixedUpdateTimer.Restart();
    }

    public interface IFixedTimeStepRepository
    {
        Stopwatch FixedUpdateTimer { get; set; }

        bool IsUpdateFrame { get; }

        void RestartTimer();
    }
}
