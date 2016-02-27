// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Net.Communication
{
	public class PacketQueue : IPacketQueue
	{
		private readonly Queue<IPacket> _internalQueue;
		private readonly object _locker;

		public int QueuedPacketCount { get { return _internalQueue.Count; } }

		public PacketQueue()
		{
			_internalQueue = new Queue<IPacket>();
			_locker = new object();
		}

		public void EnqueuePacketForHandling(IPacket pkt)
		{
			lock (_locker)
				_internalQueue.Enqueue(pkt);
		}

		public IPacket PeekPacket()
		{
			if (QueuedPacketCount == 0)
				return new EmptyPacket();

			lock (_locker)
				return _internalQueue.Peek();
		}

		public IPacket DequeueFirstPacket()
		{
			if (QueuedPacketCount == 0)
				return new EmptyPacket();

			lock (_locker)
				return _internalQueue.Dequeue();
		}

		public IEnumerable<IPacket> DequeueAllPackets()
		{
			if (QueuedPacketCount == 0)
				throw new InvalidOperationException("Error: attempting to dequeue all packets when the queue is empty!");

			IEnumerable<IPacket> ret;

			lock (_locker)
			{
				ret = _internalQueue.ToArray();
				_internalQueue.Clear();
			}

			return ret;
		}
	}
}
