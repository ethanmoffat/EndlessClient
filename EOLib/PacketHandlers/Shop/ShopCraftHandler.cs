using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Shop
{
    [AutoMappedType]
    public class ShopCraftHandler : InGameOnlyPacketHandler<ShopCreateServerPacket>
    {
        private const int ShopCraftSfxId = 27;

        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.Shop;

        public override PacketAction Action => PacketAction.Create;

        public ShopCraftHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICharacterInventoryRepository characterInventoryRepository,
                                IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(ShopCreateServerPacket packet)
        {

            foreach (var item in packet.Ingredients)
            {
                if (item.Id == 0)
                    break;

                _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == item.Id)
                    .Match(
                        some: existing =>
                        {
                            _characterInventoryRepository.ItemInventory.Remove(existing);
                            if (item.Amount > 0)
                                _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(item.Amount));
                        },
                        none: () =>
                        {
                            if (item.Amount > 0)
                                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(item.Id, item.Amount));
                        });
            }

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(ShopCraftSfxId);

            _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == packet.CraftItemId)
                .Match(
                    some: existing =>
                    {
                        _characterInventoryRepository.ItemInventory.Remove(existing);
                        _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(existing.Amount + 1));
                    },
                    none: () => _characterInventoryRepository.ItemInventory.Add(new InventoryItem(packet.CraftItemId, 1)));

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, packet.Weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, packet.Weight.Max);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }
}