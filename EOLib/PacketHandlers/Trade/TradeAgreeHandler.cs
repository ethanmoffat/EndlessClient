using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Trade;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;

namespace EOLib.PacketHandlers.Trade
{
    /// <summary>
    /// Other party agrees to a trade
    /// </summary>
    [AutoMappedType]
    public class TradeAgreeHandler : InGameOnlyPacketHandler
    {
        private readonly ITradeRepository _tradeRepository;

        public override PacketFamily Family => PacketFamily.Trade;

        public override PacketAction Action => PacketAction.Agree;

        public TradeAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                 ITradeRepository tradeRepository)
            : base(playerInfoProvider)
        {
            _tradeRepository = tradeRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var otherPlayerId = packet.ReadShort();
            var otherPlayerAgrees = packet.ReadChar() != 0;

            _tradeRepository.SomeWhen(x => x.PlayerOneOffer.PlayerID == otherPlayerId)
                .Map(x => x.PlayerOneOffer = x.PlayerOneOffer.WithAgrees(otherPlayerAgrees))
                .Or(() => _tradeRepository.PlayerTwoOffer = _tradeRepository.PlayerTwoOffer.WithAgrees(otherPlayerAgrees));

            return true;
        }
    }
}
