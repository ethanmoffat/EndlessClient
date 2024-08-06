using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;

namespace EOLib.PacketHandlers.Shop
{
    public abstract class ShopTradeHandler<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        private const byte BuySellSfxId = 26;

        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.Shop;

        protected ShopTradeHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICharacterRepository characterRepository,
                                   ICharacterInventoryRepository characterInventoryRepository,
                                   IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _soundNotifiers = soundNotifiers;
        }

        protected void Handle(int remaining, int itemId, int acquired, Weight weight)
        {
            if (Action == PacketAction.Buy)
            {
                var gold = new InventoryItem(1, remaining);
                _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
                _characterInventoryRepository.ItemInventory.Add(gold);

                var shopBuy = new InventoryItem(itemId, acquired);
                _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == itemId)
                    .Match(
                        some: existing =>
                        {
                            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == itemId);
                            _characterInventoryRepository.ItemInventory.Add(shopBuy.WithAmount(existing.Amount + shopBuy.Amount));
                        },
                        none: () => _characterInventoryRepository.ItemInventory.Add(shopBuy));
            }
            else if (Action == PacketAction.Sell)
            {
                var gold = new InventoryItem(1, acquired);
                _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
                _characterInventoryRepository.ItemInventory.Add(gold);

                var itemSold = new InventoryItem(itemId, remaining);
                _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == itemId);
                if (itemSold.Amount > 0)
                    _characterInventoryRepository.ItemInventory.Add(itemSold);
            }
            else
            {
                return;
            }

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(BuySellSfxId);

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, weight.Max);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
        }
    }

    [AutoMappedType]
    public class ShopBuyHandler : ShopTradeHandler<ShopBuyServerPacket>
    {
        public override PacketAction Action => PacketAction.Buy;

        public ShopBuyHandler(IPlayerInfoProvider playerInfoProvider,
                              ICharacterRepository characterRepository,
                              ICharacterInventoryRepository characterInventoryRepository,
                              IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider, characterRepository, characterInventoryRepository, soundNotifiers)
        {
        }

        public override bool HandlePacket(ShopBuyServerPacket packet)
        {
            Handle(packet.GoldAmount, packet.BoughtItem.Id, packet.BoughtItem.Amount, packet.Weight);
            return true;
        }
    }

    [AutoMappedType]
    public class ShopSellHandler : ShopTradeHandler<ShopSellServerPacket>
    {
        public override PacketAction Action => PacketAction.Sell;

        public ShopSellHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository characterInventoryRepository,
                              IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider, characterRepository, characterInventoryRepository, soundNotifiers)
        {
        }

        public override bool HandlePacket(ShopSellServerPacket packet)
        {
            Handle(packet.SoldItem.Amount, packet.SoldItem.Id, packet.GoldAmount, packet.Weight);
            return true;
        }
    }
}
