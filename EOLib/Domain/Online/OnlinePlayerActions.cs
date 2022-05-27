using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Online
{
    [AutoMappedType]
    public class OnlinePlayerActions : IOnlinePlayerActions
    {
        private readonly IPacketSendService _packetSendService;

        public OnlinePlayerActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void RequestOnlinePlayers(bool fullList)
        {
            var packet = new PacketBuilder(PacketFamily.Players, fullList ? PacketAction.Request : PacketAction.List).Build();
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IOnlinePlayerActions
    {
        void RequestOnlinePlayers(bool fullList);
    }
}
