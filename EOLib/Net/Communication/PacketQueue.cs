﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Communication
{
    public sealed class PacketQueue : IWaitablePacketQueue
    {
        private readonly Queue<IPacket> _internalQueue;
        private readonly object _locker;

        private TaskCompletionSource<bool> _enqueuedTaskCompletionSource;

        public int QueuedPacketCount => _internalQueue.Count;

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

        public void EnqueuePacketAndSignalConsumer(IPacket pkt)
        {
            EnqueuePacketForHandling(pkt);
            SetSignalResult(true);
        }

        public IPacket PeekPacket()
        {
            if (QueuedPacketCount == 0)
                throw new EmptyPacketReceivedException();

            lock (_locker)
                return _internalQueue.Peek();
        }

        public IPacket DequeueFirstPacket()
        {
            if (QueuedPacketCount == 0)
                throw new EmptyPacketReceivedException();

            lock (_locker)
                return _internalQueue.Dequeue();
        }

        public async Task<IPacket> WaitForPacketAndDequeue(int timeOut = Constants.ResponseTimeout)
        {
            if (QueuedPacketCount > 0)
                return DequeueFirstPacket();

            using (var cts = new CancellationTokenSource(timeOut))
            {
                _enqueuedTaskCompletionSource = new TaskCompletionSource<bool>();
                cts.Token.Register(() => SetSignalResult(false), false);

                var result = await _enqueuedTaskCompletionSource.Task;
                if (!result)
                    throw new EmptyPacketReceivedException();
            }

            return DequeueFirstPacket();
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

        private void SetSignalResult(bool result)
        {
#if DEBUG
            while (_enqueuedTaskCompletionSource == null)
                Thread.Sleep(100);
#endif

            if (!_enqueuedTaskCompletionSource.Task.IsCompleted)
                Task.Run(() => _enqueuedTaskCompletionSource.SetResult(result));
        }
    }
}
