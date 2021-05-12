using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Extensions;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Items
{
    [AutoMappedType]
    public class JunkItemHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _inventoryRepository;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Junk;

        public JunkItemHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository inventoryRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _inventoryRepository = inventoryRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();
            var amountRemoved = packet.ReadThree();
            var amountRemaining = packet.ReadInt();
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            var inventoryItem = _inventoryRepository.ItemInventory.OptionalSingle(x => x.ItemID == id);
            if (inventoryItem.HasValue)
            {
                _inventoryRepository.ItemInventory.Remove(inventoryItem.Value);

                if (amountRemaining > 0)
                {
                    var updatedItem = inventoryItem.Value.WithAmount(amountRemaining);
                    _inventoryRepository.ItemInventory.Add(updatedItem);
                }
            }

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            // todo: notify client for status message (see commented out _junkItem() in PacketApiCallbackManager)

            return true;
        }
    }
}
