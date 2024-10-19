using System.Threading;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Net.Communication;

namespace EOLib.Net.Connection
{
    [AutoMappedType]
    public class BackgroundReceiveActions : IBackgroundReceiveActions
    {
        private readonly INetworkClientProvider _clientProvider;
        private readonly IBackgroundReceiveTaskRepository _backgroundReceiveTaskRepository;

        public BackgroundReceiveActions(INetworkClientProvider clientProvider,
                                        IBackgroundReceiveTaskRepository backgroundReceiveThreadRepository)
        {
            _clientProvider = clientProvider;
            _backgroundReceiveTaskRepository = backgroundReceiveThreadRepository;
        }

        public void RunBackgroundReceiveLoop()
        {
            if (_backgroundReceiveTaskRepository.Task != null)
                return;

            _backgroundReceiveTaskRepository.BackgroundCancellationTokenSource = new CancellationTokenSource();
            _backgroundReceiveTaskRepository.Task = Task.Run(RunLoop, _backgroundReceiveTaskRepository.BackgroundCancellationTokenSource.Token);
        }

        public void CancelBackgroundReceiveLoop()
        {
            _backgroundReceiveTaskRepository.BackgroundCancellationTokenSource?.Cancel();
            _backgroundReceiveTaskRepository.BackgroundCancellationTokenSource?.Dispose();
            _backgroundReceiveTaskRepository.BackgroundCancellationTokenSource = null;

            _backgroundReceiveTaskRepository.Task = null;
        }

        private Task RunLoop()
        {
            return _clientProvider.NetworkClient.RunReceiveLoopAsync(_backgroundReceiveTaskRepository.BackgroundCancellationTokenSource.Token);
        }
    }
}
