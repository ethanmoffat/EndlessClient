// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;
using EOLib.Net.Communication;

namespace EOLib.Net.Connection
{
	public class BackgroundReceiveActions : IBackgroundReceiveActions
	{
		private readonly INetworkClientProvider _clientProvider;

		private Thread _backgroundThread;
		private bool _threadStarted;

		public BackgroundReceiveActions(INetworkClientProvider clientProvider)
		{
			_clientProvider = clientProvider;
			_backgroundThread = new Thread(BackgroundLoop);
		}

		public void RunBackgroundReceiveLoop()
		{
			if (_threadStarted)
				return;
			
			_backgroundThread.Start();
			_threadStarted = true;
		}

		public void CancelBackgroundReceiveLoop()
		{
			if (!_threadStarted)
				return;

			Client.CancelBackgroundReceiveLoop();
			
			_backgroundThread.Join();
			_backgroundThread = new Thread(BackgroundLoop);
			_threadStarted = false;
		}

		private async void BackgroundLoop()
		{
			await Client.RunReceiveLoopAsync();
		}

		private INetworkClient Client { get { return _clientProvider.NetworkClient; } }
	}
}
