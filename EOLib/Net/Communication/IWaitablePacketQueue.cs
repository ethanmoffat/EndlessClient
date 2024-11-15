using System.Threading.Tasks;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Communication
{
    public interface IWaitablePacketQueue : IPacketQueue
    {
        void EnqueuePacketAndSignalConsumer(IPacket packet);

        Task<IPacket> WaitForPacketAndDequeue(int timeOut = TimeoutConstants.ResponseTimeout);
    }
}
