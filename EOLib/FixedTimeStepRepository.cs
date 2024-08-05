using AutomaticTypeMapper;

namespace EOLib
{
    [AutoMappedType(IsSingleton = true)]
    public class FixedTimeStepRepository : IFixedTimeStepRepository
    {
        public const double TICK_TIME_MS = 10.0;

        public ulong TickCount { get; private set; }

        public bool IsWalkUpdateFrame => TickCount % 4 == 0;

        public void Tick(uint ticks = 1)
        {
            TickCount += ticks;
        }
    }

    public interface IFixedTimeStepRepository
    {
        public ulong TickCount { get; }

        bool IsWalkUpdateFrame { get; }

        void Tick(uint ticks = 1);
    }
}