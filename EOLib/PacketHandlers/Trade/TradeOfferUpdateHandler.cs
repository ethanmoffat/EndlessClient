using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Trade;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System.Linq;

namespace EOLib.PacketHandlers.Trade
{
    public abstract class TradeOfferUpdateHandler<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        protected readonly ITradeRepository _tradeRepository;

        public override PacketFamily Family => PacketFamily.Trade;

        protected TradeOfferUpdateHandler(IPlayerInfoProvider playerInfoProvider,
                                          ITradeRepository tradeRepository)
            : base(playerInfoProvider)
        {
            _tradeRepository = tradeRepository;
        }

        protected void Handle(TradeItemData data)
        {
            var player1Id = data.PartnerPlayerId;
            var player1Items = data.PartnerItems.Select(x => new InventoryItem(x.Id, x.Amount)).ToList();

            var player2Id = data.YourPlayerId;
            var player2Items = data.YourItems.Select(x => new InventoryItem(x.Id, x.Amount)).ToList();

            _tradeRepository.SomeWhen(x => x.PlayerOneOffer.PlayerID == player1Id)
                .Match(some: x =>
                    {
                        x.PlayerOneOffer = x.PlayerOneOffer.WithItems(player1Items);
                        x.PlayerTwoOffer = x.PlayerTwoOffer.WithItems(player2Items);
                    },
                    none: () =>
                    {
                        var x = _tradeRepository;
                        x.PlayerOneOffer = x.PlayerOneOffer.WithItems(player2Items);
                        x.PlayerTwoOffer = x.PlayerTwoOffer.WithItems(player1Items);
                    });

            _tradeRepository.PlayerOneOffer = _tradeRepository.PlayerOneOffer.WithAgrees(false);
            _tradeRepository.PlayerTwoOffer = _tradeRepository.PlayerTwoOffer.WithAgrees(false);
        }
    }
}