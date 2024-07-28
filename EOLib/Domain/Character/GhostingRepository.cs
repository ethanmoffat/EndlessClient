using AutomaticTypeMapper;
using Optional;
using System.Diagnostics;

namespace EOLib.Domain.Character
{
    [AutoMappedType(IsSingleton = true)]
    public sealed class GhostingRepository : IGhostingProvider, IGhostingRepository
    {
        public bool GhostedRecently => GhostStartTime.Elapsed.TotalMilliseconds > 0 && !GhostStartTime.IsRunning;

        public bool GhostCompleted { get; set; }

        public Stopwatch GhostStartTime { get; set; }

        public Option<Character> GhostTarget { get; set; }

        public GhostingRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            GhostCompleted = false;
            GhostStartTime = new Stopwatch();
            GhostTarget = Option.None<Character>();
        }
    }

    public interface IGhostingRepository : IGhostingProvider
    {
        new bool GhostCompleted { get; set; }

        new Stopwatch GhostStartTime { get; set; }

        new Option<Character> GhostTarget { get; set; }
    }

    public interface IGhostingProvider : IResettable
    {
        bool GhostedRecently { get; }

        bool GhostCompleted { get; }

        Stopwatch GhostStartTime { get; }

        Option<Character> GhostTarget { get; }
    }
}