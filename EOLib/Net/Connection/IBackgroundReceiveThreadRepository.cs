using System.Threading;
using AutomaticTypeMapper;

namespace EOLib.Net.Connection
{
    public interface IBackgroundReceiveThreadRepository
    {
        Thread BackgroundThreadObject { get; set; }

        bool BackgroundThreadRunning { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class BackgroundReceiveThreadRepository : IBackgroundReceiveThreadRepository
    {
        public Thread BackgroundThreadObject { get; set; }

        public bool BackgroundThreadRunning { get; set; }
    }
}
