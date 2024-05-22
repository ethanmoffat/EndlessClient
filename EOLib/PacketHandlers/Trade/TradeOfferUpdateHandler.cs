using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Trade;
using EOLib.IO.Repositories;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using Optional.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// Either player makes an update to their offer
    /// </summary>
    [AutoMappedType]
    public class TradeReplyHandler : TradeOfferUpdateHandler<TradeReplyServerPacket>
    {
        public override PacketAction Action => PacketAction.Reply;

        public TradeReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                 ITradeRepository tradeRepository)
            : base(playerInfoProvider, tradeRepository)
        {
        }

        public override bool HandlePacket(TradeReplyServerPacket packet)
        {
            Handle(packet.TradeData);
            return true;
        }
    }

    /// <summary>
    /// Trade completed
    /// </summary>
    [AutoMappedType]
    public class TradeUseHandler : TradeOfferUpdateHandler<TradeUseServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IEnumerable<ITradeEventNotifier> _tradeEventNotifiers;

        public override PacketAction Action => PacketAction.Use;

        public TradeUseHandler(IPlayerInfoProvider playerInfoProvider,
                               ITradeRepository tradeRepository,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository characterInventoryRepository,
                               IEIFFileProvider eifFileProvider,
                               IEnumerable<ITradeEventNotifier> tradeEventNotifiers)
            : base(playerInfoProvider, tradeRepository)
        {
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _eifFileProvider = eifFileProvider;
            _tradeEventNotifiers = tradeEventNotifiers;
        }

        public override bool HandlePacket(TradeUseServerPacket packet)
        {
            Handle(packet.TradeData);

            var (removeItems, addItems) = _tradeRepository
                .SomeWhen(x => x.PlayerOneOffer.PlayerID == _characterRepository.MainCharacter.ID)
                .Match(some: x => (x.PlayerOneOffer.Items, x.PlayerTwoOffer.Items),
                       none: () => (_tradeRepository.PlayerTwoOffer.Items, _tradeRepository.PlayerOneOffer.Items));

            var stats = _characterRepository.MainCharacter.Stats;
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

                stats = stats.WithNewStat(CharacterStat.Weight, stats[CharacterStat.Weight] - _eifFileProvider.EIFFile[removedItem.ItemID].Weight * removedItem.Amount);
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

                stats = stats.WithNewStat(CharacterStat.Weight, stats[CharacterStat.Weight] + _eifFileProvider.EIFFile[newItem.ItemID].Weight * newItem.Amount);
            }

            if (stats[CharacterStat.Weight] < 0)
                stats = stats.WithNewStat(CharacterStat.Weight, 0);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            foreach (var notifier in _tradeEventNotifiers)
                notifier.NotifyTradeClose(cancel: false);

            return true;
        }
    }
}
