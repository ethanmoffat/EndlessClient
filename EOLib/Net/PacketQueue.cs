// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Net
{
	public class PacketQueue : IPacketQueue
	{
		private readonly Queue<Packet> _internalQueue;
		private readonly object _locker;

		public int QueuedPacketCount { get { return _internalQueue.Count; } }

		public PacketQueue()
		{
			_internalQueue = new Queue<Packet>();
			_locker = new object();
		}

		public void EnqueuePacketForHandling(Packet pkt)
		{
			lock (_locker)
				_internalQueue.Enqueue(pkt);
		}

		public Packet PeekPacket()
		{
			if (QueuedPacketCount == 0)
				return Packet.Empty;

			lock (_locker)
				return _internalQueue.Peek();
		}

		public Packet DequeueFirstPacket()
		{
			if (QueuedPacketCount == 0)
				return Packet.Empty;

			lock (_locker)
				return _internalQueue.Dequeue();
		}

		public IEnumerable<Packet> DequeueAllPackets()
		{
			if (QueuedPacketCount == 0)
				throw new InvalidOperationException("Error: attempting to dequeue all packets when the queue is empty!");

			IEnumerable<Packet> ret;

			lock (_locker)
			{
				ret = _internalQueue.ToArray();
				_internalQueue.Clear();
			}

			return ret;
		}
	}
}
