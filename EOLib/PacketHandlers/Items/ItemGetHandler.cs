using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when a player picks up an item
    /// </summary>
    [AutoMappedType]
    public class ItemGetHandler : InGameOnlyPacketHandler<ItemGetServerPacket>
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

        public override bool HandlePacket(ItemGetServerPacket packet)
        {

            var existingInventoryItem = _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == packet.TakenItem.Id);
            existingInventoryItem.MatchSome(x => _characterInventoryRepository.ItemInventory.Remove(x));

            existingInventoryItem.Map(x => x.WithAmount(x.Amount + packet.TakenItem.Amount))
                .Match(some: _characterInventoryRepository.ItemInventory.Add,
                       none: () => _characterInventoryRepository.ItemInventory.Add(new InventoryItem(packet.TakenItem.Id, packet.TakenItem.Amount)));

            var newStats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, packet.Weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, packet.Weight.Max);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(newStats);

            if (_mapStateRepository.MapItems.ContainsKey(packet.TakenItemIndex))
                _mapStateRepository.MapItems.Remove(_mapStateRepository.MapItems[packet.TakenItemIndex]);

            foreach (var notifier in _mainCharacterEventNotifiers)
            {
                notifier.TakeItemFromMap(packet.TakenItem.Id, packet.TakenItem.Amount);
            }

            return true;
        }
    }
}