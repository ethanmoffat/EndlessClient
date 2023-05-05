using AutomaticTypeMapper;
using System;
using System.Diagnostics;

namespace EOLib
{
    public interface IGameStartTimeProvider
    {
        DateTime StartTime { get; }

        Stopwatch Elapsed { get; }

        int TimeStamp { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GameStartTimeRepository : IGameStartTimeProvider
    {
        public DateTime StartTime { get; } = DateTime.UtcNow;

        public Stopwatch Elapsed { get; } = Stopwatch.StartNew();

        public int TimeStamp => StartTime.ToEOTimeStamp(Elapsed.ElapsedTicks);
    }
}
