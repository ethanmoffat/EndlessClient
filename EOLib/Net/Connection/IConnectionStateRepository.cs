// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net.Connection
{
	public interface IConnectionStateRepository
	{
		bool NeedsReconnect { get; set; }
	}

	public interface IConnectionStateProvider
	{
		bool NeedsReconnect { get; }
	}

	public class ConnectionStateRepository : IConnectionStateRepository, IConnectionStateProvider
	{
		public bool NeedsReconnect { get; set; }
	}
}
