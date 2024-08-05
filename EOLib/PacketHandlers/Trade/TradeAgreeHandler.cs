using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Trade;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Trade
{
    /// <summary>
    /// Other party agrees to a trade
    /// </summary>
    [AutoMappedType]
    public class TradeAgreeHandler : InGameOnlyPacketHandler<TradeAgreeServerPacket>
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ITradeRepository _tradeRepository;

        public override PacketFamily Family => PacketFamily.Trade;

        public override PacketAction Action => PacketAction.Agree;

        public TradeAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                 ITradeRepository tradeRepository)
            : base(playerInfoProvider)
        {
            _playerInfoProvider = playerInfoProvider;
            _tradeRepository = tradeRepository;
        }

        public override bool HandlePacket(TradeAgreeServerPacket packet)
        {
            _tradeRepository.SomeWhen(x => x.PlayerOneOffer.PlayerID != _playerInfoProvider.PlayerID)
                .Map(x => x.PlayerOneOffer = x.PlayerOneOffer.WithAgrees(packet.Agree))
                .Or(() => _tradeRepository.PlayerTwoOffer = _tradeRepository.PlayerTwoOffer.WithAgrees(packet.Agree));
            return true;
        }
    }
}