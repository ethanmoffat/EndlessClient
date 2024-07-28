using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Trade
{
    /// <summary>
    /// Another player requests a trade
    /// </summary>
    [AutoMappedType]
    public class TradeRequestHandler : InGameOnlyPacketHandler<TradeRequestServerPacket>
    {
        private readonly IEnumerable<ITradeEventNotifier> _tradeEventNotifiers;

        public override PacketFamily Family => PacketFamily.Trade;

        public override PacketAction Action => PacketAction.Request;

        public TradeRequestHandler(IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<ITradeEventNotifier> tradeEventNotifiers)
            : base(playerInfoProvider)
        {
            _tradeEventNotifiers = tradeEventNotifiers;
        }

        public override bool HandlePacket(TradeRequestServerPacket packet)
        {
            foreach (var notifier in _tradeEventNotifiers)
                notifier.NotifyTradeRequest(packet.PartnerPlayerId, packet.PartnerPlayerName);

            return true;
        }
    }
}