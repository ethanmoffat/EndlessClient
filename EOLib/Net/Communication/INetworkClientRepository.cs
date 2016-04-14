// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net.Communication
{
	public interface INetworkClientRepository : INetworkClientDisposer
	{
		INetworkClient NetworkClient { get; set; }
	}

	public interface INetworkClientProvider
	{
		INetworkClient NetworkClient { get; }
	}

	public sealed class NetworkClientRepository : INetworkClientProvider, INetworkClientRepository
	{
		public INetworkClient NetworkClient { get; set; }
		
		public void Dispose()
		{
			if (NetworkClient != null)
				NetworkClient.Dispose();
		}
	}
}
