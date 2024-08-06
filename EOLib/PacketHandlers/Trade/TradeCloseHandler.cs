using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Trade;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Trade
{
    /// <summary>
    /// Other party agrees to a trade
    /// </summary>
    [AutoMappedType]
    public class TradeCloseHandler : InGameOnlyPacketHandler<TradeCloseServerPacket>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IEnumerable<ITradeEventNotifier> _tradeEventNotifiers;

        public override PacketFamily Family => PacketFamily.Trade;

        public override PacketAction Action => PacketAction.Close;

        public TradeCloseHandler(IPlayerInfoProvider playerInfoProvider,
                                 ITradeRepository tradeRepository,
                                 IEnumerable<ITradeEventNotifier> tradeEventNotifiers)
            : base(playerInfoProvider)
        {
            _tradeRepository = tradeRepository;
            _tradeEventNotifiers = tradeEventNotifiers;
        }

        public override bool HandlePacket(TradeCloseServerPacket packet)
        {
            foreach (var notifier in _tradeEventNotifiers)
                notifier.NotifyTradeClose(cancel: true);

            _tradeRepository.ResetState();

            return true;
        }
    }
}
