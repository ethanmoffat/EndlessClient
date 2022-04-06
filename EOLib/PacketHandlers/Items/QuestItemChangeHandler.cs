using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Items
{
    public abstract class QuestItemChangeHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _inventoryRepository;

        public override PacketFamily Family => PacketFamily.Item;

        protected QuestItemChangeHandler(IPlayerInfoProvider playerInfoProvider,
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
            var amount = packet.ReadThree();
            var weight = packet.ReadChar();

            var inventoryItem = _inventoryRepository.ItemInventory
                .SingleOrNone(x => x.ItemID == id)
                .Match(x => x, () => new InventoryItem(id, 0));
            _inventoryRepository.ItemInventory.Remove(inventoryItem);

            if (amount > 0)
            {
                var amountRemaining = Action == PacketAction.Kick
                    ? amount
                    : inventoryItem.Amount + amount;

                if (amountRemaining > 0)
                {
                    _inventoryRepository.ItemInventory.Add(inventoryItem.WithAmount(amountRemaining));
                }
            }

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, weight);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }

    [AutoMappedType]
    public class ItemObtainHandler : QuestItemChangeHandler
    {
        public override PacketAction Action => PacketAction.Obtain;

        public ItemObtainHandler(IPlayerInfoProvider playerInfoProvider,
            ICharacterRepository characterRepository,
            ICharacterInventoryRepository inventoryRepository)
            : base(playerInfoProvider, characterRepository, inventoryRepository)
        {
        }
    }

    [AutoMappedType]
    public class ItemKickHandler : QuestItemChangeHandler
    {
        public override PacketAction Action => PacketAction.Kick;

        public ItemKickHandler(IPlayerInfoProvider playerInfoProvider,
            ICharacterRepository characterRepository,
            ICharacterInventoryRepository inventoryRepository)
            : base(playerInfoProvider, characterRepository, inventoryRepository)
        {
        }
    }
}
