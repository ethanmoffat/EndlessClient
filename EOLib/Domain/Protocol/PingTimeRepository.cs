using System.Diagnostics;
using AutomaticTypeMapper;

namespace EOLib.Domain.Protocol
{
    public interface IPingTimeRepository
    {
        Stopwatch RequestTimer { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PingTimeRepository : IPingTimeRepository
    {
        public Stopwatch RequestTimer { get; set; }

        public PingTimeRepository()
        {
            RequestTimer = new Stopwatch();
        }
    }
}