using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public interface IPacketSendService
    {
        void SendPacket(IPacket packet);

        Task SendPacketAsync(IPacket packet);

        Task<IPacket> SendRawPacketAndWaitAsync(IPacket packet);

        Task<IPacket> SendEncodedPacketAndWaitAsync(IPacket packet);
    }
}
