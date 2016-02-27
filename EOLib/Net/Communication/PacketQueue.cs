// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Net.Communication
{
	public class PacketQueue : IPacketQueue
	{
		private readonly Queue<OldPacket> _internalQueue;
		private readonly object _locker;

		public int QueuedPacketCount { get { return _internalQueue.Count; } }

		public PacketQueue()
		{
			_internalQueue = new Queue<OldPacket>();
			_locker = new object();
		}

		public void EnqueuePacketForHandling(OldPacket pkt)
		{
			lock (_locker)
				_internalQueue.Enqueue(pkt);
		}

		public OldPacket PeekPacket()
		{
			if (QueuedPacketCount == 0)
				return OldPacket.Empty;

			lock (_locker)
				return _internalQueue.Peek();
		}

		public OldPacket DequeueFirstPacket()
		{
			if (QueuedPacketCount == 0)
				return OldPacket.Empty;

			lock (_locker)
				return _internalQueue.Dequeue();
		}

		public IEnumerable<OldPacket> DequeueAllPackets()
		{
			if (QueuedPacketCount == 0)
				throw new InvalidOperationException("Error: attempting to dequeue all packets when the queue is empty!");

			IEnumerable<OldPacket> ret;

			lock (_locker)
			{
				ret = _internalQueue.ToArray();
				_internalQueue.Clear();
			}

			return ret;
		}
	}
}
