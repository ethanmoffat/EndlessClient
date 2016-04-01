// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
	public class BackgroundReceiveActions : IBackgroundReceiveActions
	{
		private readonly INetworkClientProvider _clientProvider;

		public BackgroundReceiveActions(INetworkClientProvider clientProvider)
		{
			_clientProvider = clientProvider;
		}

		public async Task RunBackgroundReceiveLoopAsync()
		{
			await Client.RunReceiveLoopAsync();
		}

		public void CancelBackgroundReceiveLoop()
		{
			Client.CancelBackgroundReceiveLoop();
		}

		private INetworkClient Client { get { return _clientProvider.NetworkClient; } }
	}
}
