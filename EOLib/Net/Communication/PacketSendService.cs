using System.Threading.Tasks;
using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Communication
{
    [AutoMappedType]
    public class PacketSendService : IPacketSendService
    {
        private readonly INetworkClientProvider _networkClientProvider;
        private readonly IPacketQueueProvider _packetQueueProvider;

        public PacketSendService(INetworkClientProvider networkClientProvider,
                                 IPacketQueueProvider packetQueueProvider)
        {
            _networkClientProvider = networkClientProvider;
            _packetQueueProvider = packetQueueProvider;
        }

        public void SendPacket(IPacket packet)
        {
            var bytes = Client.Send(packet);
            if (bytes == 0)
                throw new NoDataSentException();
        }

        public async Task SendPacketAsync(IPacket packet)
        {
            var bytes = await Client.SendAsync(packet);
            if (bytes == 0)
                throw new NoDataSentException();
        }

        public async Task<IPacket> SendRawPacketAndWaitAsync(IPacket packet)
        {
            var bytes = await Client.SendRawPacketAsync(packet);
            if (bytes == 0)
                throw new NoDataSentException();

            return await InBandQueue.WaitForPacketAndDequeue((int)Client.ReceiveTimeout.TotalMilliseconds);
        }

        public async Task<IPacket> SendEncodedPacketAndWaitAsync(IPacket packet)
        {
            var bytes = await Client.SendAsync(packet);
            if (bytes == 0)
                throw new NoDataSentException();

            return await InBandQueue.WaitForPacketAndDequeue((int)Client.ReceiveTimeout.TotalMilliseconds);
        }

        private INetworkClient Client => _networkClientProvider.NetworkClient;

        private IWaitablePacketQueue InBandQueue => _packetQueueProvider.HandleInBandPacketQueue;
    }
}