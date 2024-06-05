using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Trade;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Trade
{
    /// <summary>
    /// Trade request is accepted
    /// </summary>
    [AutoMappedType]
    public class TradeOpenHandler : InGameOnlyPacketHandler<TradeOpenServerPacket>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IEnumerable<ITradeEventNotifier> _tradeEventNotifiers;

        public override PacketFamily Family => PacketFamily.Trade;

        public override PacketAction Action => PacketAction.Open;

        public TradeOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                ITradeRepository tradeRepository,
                                IEnumerable<ITradeEventNotifier> tradeEventNotifiers)
            : base(playerInfoProvider)
        {
            _tradeRepository = tradeRepository;
            _tradeEventNotifiers = tradeEventNotifiers;
        }

        public override bool HandlePacket(TradeOpenServerPacket packet)
        {
            _tradeRepository.PlayerOneOffer = _tradeRepository.PlayerOneOffer
                .WithAgrees(false)
                .WithPlayerID(packet.PartnerPlayerId)
                .WithPlayerName(packet.PartnerPlayerName)
                .WithItems(new List<InventoryItem>());

            _tradeRepository.PlayerTwoOffer = _tradeRepository.PlayerTwoOffer
                .WithAgrees(false)
                .WithPlayerID(packet.YourPlayerId)
                .WithPlayerName(packet.YourPlayerName)
                .WithItems(new List<InventoryItem>());

            foreach (var notifier in _tradeEventNotifiers)
                notifier.NotifyTradeAccepted();

            return true;
        }
    }
}
