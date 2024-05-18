using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when a player junks an item
    /// </summary>
    [AutoMappedType]
    public class ItemJunkHandler : InGameOnlyPacketHandler<ItemJunkServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _inventoryRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Junk;

        public ItemJunkHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository inventoryRepository,
                               IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _inventoryRepository = inventoryRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(ItemJunkServerPacket packet)
        {
            var inventoryItem = _inventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == packet.JunkedItem.Id);
            inventoryItem.MatchSome(x => _inventoryRepository.ItemInventory.Remove(x));

            if (packet.RemainingAmount > 0)
            {
                inventoryItem.Map(x => x.WithAmount(packet.RemainingAmount))
                    .MatchSome(x => _inventoryRepository.ItemInventory.Add(x));
            }

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, packet.Weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, packet.Weight.Max);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.JunkItem(packet.JunkedItem.Id, packet.JunkedItem.Amount);

            return true;
        }
    }
}
