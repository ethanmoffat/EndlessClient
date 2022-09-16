using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Trade;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;

namespace EOLib.PacketHandlers.Trade
{
    /// <summary>
    /// You agree to a trade
    /// </summary>
    [AutoMappedType]
    public class TradeSpecHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ITradeRepository _tradeRepository;

        public override PacketFamily Family => PacketFamily.Trade;

        public override PacketAction Action => PacketAction.Spec;

        public TradeSpecHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterProvider characterProvider,
                                ITradeRepository tradeRepository)
            : base(playerInfoProvider)
        {
            _characterProvider = characterProvider;
            _tradeRepository = tradeRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var youAgree = packet.ReadChar() != 0;

            _tradeRepository.SomeWhen(x => x.PlayerOneOffer.PlayerID == _characterProvider.MainCharacter.ID)
                .Map(x => x.PlayerOneOffer = x.PlayerOneOffer.WithAgrees(youAgree))
                .Or(() => _tradeRepository.PlayerTwoOffer = _tradeRepository.PlayerTwoOffer.WithAgrees(youAgree));

            return true;
        }
    }
}
