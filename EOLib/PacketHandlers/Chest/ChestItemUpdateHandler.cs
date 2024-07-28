using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Optional.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Chest
{
    public abstract class ChestItemUpdateHandler<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        private readonly IChestDataRepository _chestDataRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        protected ChestItemUpdateHandler(IPlayerInfoProvider playerInfoProvider,
                                         IChestDataRepository chestDataRepository,
                                         ICharacterRepository characterRepository,
                                         ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _chestDataRepository = chestDataRepository;
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        protected void Handle(List<ThreeItem> items, ThreeItem item, Weight weight, bool addingItemFromInventory)
        {
            _chestDataRepository.Items = new HashSet<ChestItem>(items.Select((x, i) => new ChestItem(x.Id, x.Amount, i)));

            if (addingItemFromInventory)
            {
                _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == item.Id);
                if (item.Amount > 0)
                {
                    _characterInventoryRepository.ItemInventory.Add(new InventoryItem(item.Id, item.Amount));
                }
            }
            else
            {
                _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == item.Id)
                    .Match(
                        some: existing =>
                        {
                            if (item.Amount > 0 || item.Id == 1)
                            {
                                _characterInventoryRepository.ItemInventory.Remove(existing);
                                _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(existing.Amount + item.Amount));
                            }
                        },
                        none: () =>
                        {
                            if (item.Amount > 0)
                                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(item.Id, item.Amount));
                        });
            }

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, weight.Max);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
        }
    }
}