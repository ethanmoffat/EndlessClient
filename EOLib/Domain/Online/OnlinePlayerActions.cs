using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Translators;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOLib.Domain.Online
{
    [AutoMappedType]
    public class OnlinePlayerActions : IOnlinePlayerActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IPacketTranslator<OnlineListData> _onlineListPacketTranslator;

        public OnlinePlayerActions(IPacketSendService packetSendService,
                                   IPacketTranslator<OnlineListData> onlineListPacketTranslator)
        {
            _packetSendService = packetSendService;
            _onlineListPacketTranslator = onlineListPacketTranslator;
        }

        public async Task<IReadOnlyList<OnlinePlayerInfo>> GetOnlinePlayersAsync(bool fullList)
        {
            var packet = new PacketBuilder(PacketFamily.Players, fullList ? PacketAction.Request : PacketAction.List).Build();
            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);

            return _onlineListPacketTranslator.TranslatePacket(response).OnlineList;
        }
    }

    public interface IOnlinePlayerActions
    {
        Task<IReadOnlyList<OnlinePlayerInfo>> GetOnlinePlayersAsync(bool fullList);
    }
}
