using AutomaticTypeMapper;
using System.Diagnostics;

namespace EOLib
{
    public interface IGameStartTimeProvider
    {
        Stopwatch Elapsed { get; }

        int TimeStamp { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GameStartTimeRepository : IGameStartTimeProvider
    {
        public Stopwatch Elapsed { get; } = Stopwatch.StartNew();

        public int TimeStamp => Elapsed.ToEOTimeStamp();
    }
}
