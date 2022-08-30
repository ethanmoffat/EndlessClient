using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Trade;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using Optional.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Trade
{
    public abstract class TradeOfferUpdateHandler : InGameOnlyPacketHandler
    {
        protected readonly ITradeRepository _tradeRepository;

        public override PacketFamily Family => PacketFamily.Trade;

        protected TradeOfferUpdateHandler(IPlayerInfoProvider playerInfoProvider,
                                          ITradeRepository tradeRepository)
            : base(playerInfoProvider)
        {
            _tradeRepository = tradeRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var player1Id = packet.ReadShort();
            var player1Items = new List<InventoryItem>();
            while (packet.PeekByte() != 255)
            {
                player1Items.Add(new InventoryItem(packet.ReadShort(), packet.ReadInt()));
            }

            var player2Id = packet.ReadShort();
            var player2Items = new List<InventoryItem>();
            while (packet.PeekByte() != 255)
            {
                player2Items.Add(new InventoryItem(packet.ReadShort(), packet.ReadInt()));
            }

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

            return true;
        }
    }

    /// <summary>
    /// Either player makes an update to their offer
    /// </summary>
    [AutoMappedType]
    public class TradeReplyHandler : TradeOfferUpdateHandler
    {
        public override PacketAction Action => PacketAction.Reply;

        public TradeReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                 ITradeRepository tradeRepository)
            : base(playerInfoProvider, tradeRepository)
        {
        }
    }

    /// <summary>
    /// Trade completed
    /// </summary>
    public class TradeUseHandler : TradeOfferUpdateHandler
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<ITradeEventNotifier> _tradeEventNotifiers;

        public override PacketAction Action => PacketAction.Use;

        public TradeUseHandler(IPlayerInfoProvider playerInfoProvider,
                               ITradeRepository tradeRepository,
                               ICharacterProvider characterProvider,
                               ICharacterInventoryRepository characterInventoryRepository,
                               IEnumerable<ITradeEventNotifier> tradeEventNotifiers)
            : base(playerInfoProvider, tradeRepository)
        {
            _characterProvider = characterProvider;
            _characterInventoryRepository = characterInventoryRepository;
            _tradeEventNotifiers = tradeEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            base.HandlePacket(packet);

            var (removeItems, addItems) = _tradeRepository
                .SomeWhen(x => x.PlayerOneOffer.PlayerID == _characterProvider.MainCharacter.ID)
                .Match(some: x => (x.PlayerOneOffer.Items, x.PlayerTwoOffer.Items),
                       none: () => (_tradeRepository.PlayerTwoOffer.Items, _tradeRepository.PlayerOneOffer.Items));

            foreach (var removedItem in removeItems)
            {
                _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == removedItem.ItemID)
                    .MatchSome(x =>
                    {
                        _characterInventoryRepository.ItemInventory.Remove(x);
                        if (x.Amount - removedItem.Amount > 0)
                        {
                            _characterInventoryRepository.ItemInventory.Add(x.WithAmount(x.Amount - removedItem.Amount));
                        }
                    });
            }

            foreach (var newItem in addItems)
            {
                _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == newItem.ItemID)
                    .Match(some: x =>
                    {
                        _characterInventoryRepository.ItemInventory.Remove(x);
                        _characterInventoryRepository.ItemInventory.Add(x.WithAmount(x.Amount + newItem.Amount));
                    },
                    none: () =>
                    {
                        _characterInventoryRepository.ItemInventory.Add(newItem);
                    });
            }

            foreach (var notifier in _tradeEventNotifiers)
                notifier.NotifyTradeClose(cancel: false);

            return true;
        }
    }
}
