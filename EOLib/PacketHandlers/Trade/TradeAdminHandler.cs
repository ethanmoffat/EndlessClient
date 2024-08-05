using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Trade;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Trade
{
    [AutoMappedType]
    public class TradeAdminHandler : TradeOfferUpdateHandler<TradeAdminServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Trade;

        public override PacketAction Action => PacketAction.Admin;

        public TradeAdminHandler(IPlayerInfoProvider playerInfoProvider,
                                 ITradeRepository tradeRepository)
            : base(playerInfoProvider, tradeRepository)
        {
        }

        public override bool HandlePacket(TradeAdminServerPacket packet)
        {
            Handle(packet.TradeData);
            return true;
        }
    }
}