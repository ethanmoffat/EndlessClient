using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public interface IWaitablePacketQueue : IPacketQueue
    {
        void EnqueuePacketAndSignalConsumer(IPacket packet);

        Task<IPacket> WaitForPacketAndDequeue(int timeOut = Constants.ResponseTimeout);
    }
}