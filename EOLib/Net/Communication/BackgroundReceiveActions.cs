// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;

namespace EOLib.Net.Communication
{
	public class BackgroundReceiveActions : IBackgroundReceiveActions
	{
		private readonly INetworkClientProvider _clientProvider;

		private Thread _backgroundThread;

		public BackgroundReceiveActions(INetworkClientProvider clientProvider)
		{
			_clientProvider = clientProvider;
			_backgroundThread = new Thread(BackgroundLoop);
		}

		public void RunBackgroundReceiveLoop()
		{
			_backgroundThread.Start();
		}

		public void CancelBackgroundReceiveLoop()
		{
			Client.CancelBackgroundReceiveLoop();
			_backgroundThread.Join();
			_backgroundThread = new Thread(BackgroundLoop);
		}

		private async void BackgroundLoop()
		{
			await Client.RunReceiveLoopAsync();
		}

		private INetworkClient Client { get { return _clientProvider.NetworkClient; } }
	}
}
