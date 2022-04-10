using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;

namespace EOLib.PacketHandlers.Shop
{
    [AutoMappedType]
    public class ShopCraftHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Shop;

        public override PacketAction Action => PacketAction.Create;

        public ShopCraftHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var itemId = packet.ReadShort();
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            while (packet.ReadPosition < packet.Length)
            {
                if (packet.PeekShort() == 0) break;

                var nextItemId = packet.ReadShort();
                var nextItemAmount = packet.ReadInt();

                _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == nextItemId)
                    .Match(
                        some: existing =>
                        {
                            _characterInventoryRepository.ItemInventory.Remove(existing);
                            if (nextItemAmount > 0)
                                _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(nextItemAmount));
                        },
                        none: () =>
                        {
                            if (nextItemAmount > 0)
                                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(nextItemId, nextItemAmount));
                        });
            }

            _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == itemId)
                .Match(
                    some: existing =>
                    {
                        _characterInventoryRepository.ItemInventory.Remove(existing);
                        _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(existing.Amount + 1));
                    },
                    none: () => _characterInventoryRepository.ItemInventory.Add(new InventoryItem(itemId, 1)));

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }
}
