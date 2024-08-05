using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when the main character drops an item
    /// </summary>
    [AutoMappedType]
    public class ItemDropHandler : InGameOnlyPacketHandler<ItemDropServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _inventoryRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Drop;

        public ItemDropHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository inventoryRepository,
                               ICurrentMapStateRepository currentMapStateRepository,
                               IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _inventoryRepository = inventoryRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(ItemDropServerPacket packet)
        {
            _inventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == packet.DroppedItem.Id);
            if (packet.RemainingAmount > 0 || packet.DroppedItem.Id == 1)
                _inventoryRepository.ItemInventory.Add(new InventoryItem(packet.DroppedItem.Id, packet.RemainingAmount));

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, packet.Weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, packet.Weight.Max);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            var mapItem = new MapItem(packet.ItemIndex, packet.DroppedItem.Id, packet.Coords.X, packet.Coords.Y, packet.DroppedItem.Amount)
                .WithDropTime(Option.Some(DateTime.Now.AddSeconds(-5)));
            _currentMapStateRepository.MapItems.Add(mapItem);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.DropItem(packet.DroppedItem.Id, packet.DroppedItem.Amount);

            return true;
        }
    }
}