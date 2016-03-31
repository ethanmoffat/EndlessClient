// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
	public sealed class PacketQueue : IPacketQueue
	{
		private readonly Queue<IPacket> _internalQueue;
		private readonly object _locker;
		private readonly AutoResetEvent _enqueuedEvent;

		public int QueuedPacketCount { get { return _internalQueue.Count; } }

		public PacketQueue()
		{
			_internalQueue = new Queue<IPacket>();
			_locker = new object();

			_enqueuedEvent = new AutoResetEvent(false);
		}

		public void EnqueuePacketForHandling(IPacket pkt)
		{
			lock (_locker)
				_internalQueue.Enqueue(pkt);
			_enqueuedEvent.Set();
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

		public async Task<IPacket> WaitForPacketAndDequeue(int timeOut = Constants.ResponseTimeout)
		{
			if (QueuedPacketCount > 0)
				return DequeueFirstPacket();

			return await Task.Run(() =>
			{
				if (!_enqueuedEvent.WaitOne(timeOut))
					return new EmptyPacket();

				return DequeueFirstPacket();
			});
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

		public void Dispose()
		{
			_enqueuedEvent.Dispose();
		}
	}
}
