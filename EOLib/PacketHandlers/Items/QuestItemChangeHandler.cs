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

            var inventoryItem = _inventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == id);
            inventoryItem.MatchSome(x => _inventoryRepository.ItemInventory.Remove(x));

            if (amount > 0)
            {
                var amountRemaining = inventoryItem.Match(
                    some: x => Action == PacketAction.Kick ? x.Amount - amount : x.Amount + amount,
                    none: () => Action == PacketAction.Kick ? 0 : amount);

                if (amountRemaining > 0)
                {
                    inventoryItem.Map(x => x.WithAmount(amount))
                        .MatchSome(x => _inventoryRepository.ItemInventory.Add(x));
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
