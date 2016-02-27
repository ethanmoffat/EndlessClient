// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.Net
{
	public interface IPacketQueue
	{
		int QueuedPacketCount { get; }

		void EnqueuePacketForHandling(Packet pkt);

		Packet PeekPacket();

		Packet DequeueFirstPacket();

		IEnumerable<Packet> DequeueAllPackets();
	}
}
