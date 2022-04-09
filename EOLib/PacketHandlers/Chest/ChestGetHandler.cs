using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using Optional.Collections;

namespace EOLib.PacketHandlers.Chest
{
    /// <summary>
    /// Handler for CHEST_GET packet, sent as confirmation to character that item is being taken
    /// </summary>
    [AutoMappedType]
    public class ChestGetHandler : ChestAgreeHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Chest;

        public override PacketAction Action => PacketAction.Get;

        public ChestGetHandler(IPlayerInfoProvider playerInfoProvider,
                               IChestDataRepository chestDataRepository,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, chestDataRepository)
        {
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var itemId = packet.ReadShort();
            var amount = Action == PacketAction.Get ? packet.ReadThree() : packet.ReadInt();
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == itemId)
                .Match(
                    some: existing =>
                    {
                        _characterInventoryRepository.ItemInventory.Remove(existing);
                        if (amount > 0 || itemId == 1)
                        {
                            _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(existing.Amount + (Action == PacketAction.Get ? amount : -amount)));
                        }
                    },
                    none: () =>
                    {
                        if (amount > 0)
                            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(itemId, amount));
                    });

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return base.HandlePacket(packet);
        }
    }

    /// <summary>
    /// Handler for CHEST_REPLY packet, sent in response to main player adding an item to a chest
    /// </summary>
    [AutoMappedType]
    public class ChestReplyHandler : ChestGetHandler
    {
        public override PacketAction Action => PacketAction.Reply;

        public ChestReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                 IChestDataRepository chestDataRepository,
                                 ICharacterRepository characterRepository,
                                 ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, chestDataRepository, characterRepository, characterInventoryRepository)
        {
        }
    }
}
