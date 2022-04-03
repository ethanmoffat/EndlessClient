using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;

namespace EOLib.PacketHandlers.Interact.Shop
{
    public abstract class ShopTradeHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Shop;

        protected ShopTradeHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICharacterRepository characterRepository,
                                   ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var remaining = packet.ReadInt(); // character gold remaining on buy; item amount remaining on sell
            var itemId = packet.ReadShort();
            var acquired = packet.ReadInt(); // amount acquired on buy; gold acquired on sell
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

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
                            _characterInventoryRepository.ItemInventory.Add(shopBuy);
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
                return false;
            }

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }

    [AutoMappedType]
    public class ShopBuyHandler : ShopTradeHandler
    {
        public override PacketAction Action => PacketAction.Buy;

        public ShopBuyHandler(IPlayerInfoProvider playerInfoProvider,
                              ICharacterRepository characterRepository,
                              ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, characterRepository, characterInventoryRepository)
        {
        }
    }

    [AutoMappedType]
    public class ShopSellHandler : ShopTradeHandler
    {
        public override PacketAction Action => PacketAction.Sell;

        public ShopSellHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, characterRepository, characterInventoryRepository)
        {
        }
    }
}
