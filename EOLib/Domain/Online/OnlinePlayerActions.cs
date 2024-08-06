using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
            _packetSendService.SendPacket(fullList ? (IPacket)new PlayersRequestClientPacket() : new PlayersListClientPacket());
        }
    }

    public interface IOnlinePlayerActions
    {
        void RequestOnlinePlayers(bool fullList);
    }
}
