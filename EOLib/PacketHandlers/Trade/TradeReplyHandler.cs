using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Trade;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Trade
{
    /// <summary>
    /// Either player makes an update to their offer
    /// </summary>
    [AutoMappedType]
    public class TradeReplyHandler : TradeOfferUpdateHandler<TradeReplyServerPacket>
    {
        public override PacketAction Action => PacketAction.Reply;

        public TradeReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                 ITradeRepository tradeRepository)
            : base(playerInfoProvider, tradeRepository)
        {
        }

        public override bool HandlePacket(TradeReplyServerPacket packet)
        {
            Handle(packet.TradeData);
            return true;
        }
    }
}
