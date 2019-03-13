// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;
using AutomaticTypeMapper;
using EOLib.Net.Communication;

namespace EOLib.Net.Connection
{
    [AutoMappedType]
    public class BackgroundReceiveActions : IBackgroundReceiveActions
    {
        private readonly INetworkClientProvider _clientProvider;
        private readonly IBackgroundReceiveThreadRepository _backgroundReceiveThreadRepository;

        public BackgroundReceiveActions(INetworkClientProvider clientProvider,
                                        IBackgroundReceiveThreadRepository backgroundReceiveThreadRepository)
        {
            _clientProvider = clientProvider;
            _backgroundReceiveThreadRepository = backgroundReceiveThreadRepository;
            _backgroundReceiveThreadRepository.BackgroundThreadObject = new Thread(BackgroundLoop);
        }

        public void RunBackgroundReceiveLoop()
        {
            if (_backgroundReceiveThreadRepository.BackgroundThreadRunning)
                return;

            _backgroundReceiveThreadRepository.BackgroundThreadObject.Start();
            _backgroundReceiveThreadRepository.BackgroundThreadRunning = true;
        }

        public void CancelBackgroundReceiveLoop()
        {
            if (!_backgroundReceiveThreadRepository.BackgroundThreadRunning)
                return;

            Client.CancelBackgroundReceiveLoop();

            _backgroundReceiveThreadRepository.BackgroundThreadObject.Join();
            _backgroundReceiveThreadRepository.BackgroundThreadObject = new Thread(BackgroundLoop);
            _backgroundReceiveThreadRepository.BackgroundThreadRunning = false;
        }

        private async void BackgroundLoop()
        {
            await Client.RunReceiveLoopAsync();
        }

        private INetworkClient Client => _clientProvider.NetworkClient;
    }
}
