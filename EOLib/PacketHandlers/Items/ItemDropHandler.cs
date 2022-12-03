using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when the main character drops an item
    /// </summary>
    [AutoMappedType]
    public class ItemDropHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();
            var amountDropped = packet.ReadThree();
            var amountRemaining = packet.ReadInt();

            var uid = packet.ReadShort();
            var dropX = packet.ReadChar();
            var dropY = packet.ReadChar();

            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            _inventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == id);
            if (amountRemaining > 0 || id == 1)
                _inventoryRepository.ItemInventory.Add(new InventoryItem(id, amountRemaining));

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            var mapItem = new MapItem(uid, id, dropX, dropY, amountDropped)
                .WithDropTime(Option.Some(DateTime.Now.AddSeconds(-5)));
            _currentMapStateRepository.MapItems.Add(mapItem);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.DropItem(id, amountDropped);

            return true;
        }
    }
}
