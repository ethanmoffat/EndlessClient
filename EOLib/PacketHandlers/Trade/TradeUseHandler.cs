using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Trade;
using EOLib.IO.Repositories;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Trade
{
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
