using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when a player picks up an item
    /// </summary>
    [AutoMappedType]
    public class ItemGetHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _mapStateRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Get;

        public ItemGetHandler(IPlayerInfoProvider playerInfoProvider,
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

            var existingInventoryItem = _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == id);
            existingInventoryItem.MatchSome(x => _characterInventoryRepository.ItemInventory.Remove(x));

            existingInventoryItem.Map(x => x.WithAmount(x.Amount + amountTaken))
                .Match(some: _characterInventoryRepository.ItemInventory.Add,
                       none: () => _characterInventoryRepository.ItemInventory.Add(new InventoryItem(id, amountTaken)));

            var newStats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(newStats);

            if (_mapStateRepository.MapItems.ContainsKey(uid))
                _mapStateRepository.MapItems.Remove(_mapStateRepository.MapItems[uid]);

            foreach (var notifier in _mainCharacterEventNotifiers)
            {
                notifier.TakeItemFromMap(id, amountTaken);
            }

            return true;
        }
    }
}
