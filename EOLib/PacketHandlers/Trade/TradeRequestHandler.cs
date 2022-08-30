using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Trade;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Trade
{
    /// <summary>
    /// Another player requests a trade
    /// </summary>
    [AutoMappedType]
    public class TradeRequestHandler : InGameOnlyPacketHandler
    {
        private readonly ITradeProvider _tradeProvider;
        private readonly IEnumerable<ITradeEventNotifier> _tradeEventNotifiers;

        public override PacketFamily Family => PacketFamily.Trade;

        public override PacketAction Action => PacketAction.Request;

        public TradeRequestHandler(IPlayerInfoProvider playerInfoProvider,
                                   ITradeProvider tradeProvider,
                                   IEnumerable<ITradeEventNotifier> tradeEventNotifiers)
            : base(playerInfoProvider)
        {
            _tradeProvider = tradeProvider;
            _tradeEventNotifiers = tradeEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var sessionId = packet.ReadChar();
            var playerId = packet.ReadShort();
            var name = packet.ReadEndString();

            if (sessionId == _tradeProvider.TradeSessionID)
            {
                foreach (var notifier in _tradeEventNotifiers)
                    notifier.NotifyTradeRequest(playerId, name);
            }

            return true;
        }
    }
}
