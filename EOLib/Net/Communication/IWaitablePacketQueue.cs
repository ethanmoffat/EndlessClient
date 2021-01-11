using System;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public interface IWaitablePacketQueue : IPacketQueue
    {
        void EnqueuePacketAndSignalConsumer(IPacket packet);

        Task<IPacket> WaitForPacketAndDequeue(TimeSpan timeOut);
    }
}