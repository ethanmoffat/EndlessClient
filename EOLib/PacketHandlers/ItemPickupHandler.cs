// Original Work Copyright (c) Ethan Moffat 2014-2019

using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Extensions;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class ItemPickupHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _mapStateRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Get;

        public ItemPickupHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterInventoryRepository characterInventoryRepository,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository mapStateRepository,
                                 IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
            _characterRepository = characterRepository;
            _mapStateRepository = mapStateRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var uid = packet.ReadShort();
            var id = packet.ReadShort();
            var amountTaken = packet.ReadThree();
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            var existing = _characterInventoryRepository.ItemInventory.OptionalSingle(x => x.ItemID == id);
            if (existing.HasValue)
            {
                _characterInventoryRepository.ItemInventory.Remove(existing.Value);
                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(id, existing.Value.Amount + amountTaken));
            }
            else
            {
                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(id, amountTaken));
            }

            var newStats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(newStats);

            _mapStateRepository.MapItems.RemoveAll(x => x.UniqueID == uid);

            foreach (var notifier in _mainCharacterEventNotifiers)
            {
                notifier.TakeItemFromMap(id, amountTaken);
            }

            return true;
        }
    }
}
