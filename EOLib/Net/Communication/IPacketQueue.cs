// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
	public interface IPacketQueue : IDisposable
	{
		int QueuedPacketCount { get; }

		void EnqueuePacketForHandling(IPacket pkt);

		IPacket PeekPacket();

		IPacket DequeueFirstPacket();

		Task<IPacket> WaitForPacketAndDequeue(int timeOut = Constants.ResponseTimeout);

		IEnumerable<IPacket> DequeueAllPackets();
	}
}
