using AutomaticTypeMapper;
using System.Threading;
using System.Threading.Tasks;

namespace EOLib.Net.Connection
{
    public interface IBackgroundReceiveTaskRepository
    {
        Task Task { get; set; }

        CancellationTokenSource BackgroundCancellationTokenSource { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class BackgroundReceiveThreadRepository : IBackgroundReceiveTaskRepository
    {
        public Task Task { get; set; }

        public CancellationTokenSource BackgroundCancellationTokenSource { get; set; }
    }
}
