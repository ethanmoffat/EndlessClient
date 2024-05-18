using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace EOLib.PacketHandlers.Chest
{
    /// <summary>
    /// Handler for CHEST_GET packet, sent as confirmation to character that item is being taken
    /// </summary>
    [AutoMappedType]
    public class ChestGetHandler : InGameOnlyPacketHandler<ChestGetServerPacket>
    {
        private readonly IChestDataRepository _chestDataRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Chest;

        public override PacketAction Action => PacketAction.Get;

        public ChestGetHandler(IPlayerInfoProvider playerInfoProvider,
                               IChestDataRepository chestDataRepository,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _chestDataRepository = chestDataRepository;
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(ChestGetServerPacket packet)
        {
            Handle(packet.Items, packet.TakenItem, packet.Weight, addingItemFromInventory: false);
            return true;
        }

        protected void Handle(List<ThreeItem> items, ThreeItem item, Weight weight, bool addingItemFromInventory)
        {
            _chestDataRepository.Items = new HashSet<ChestItem>(items.Select((x, i) => new ChestItem(x.Id, x.Amount, i)));

            _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == item.Id)
                .Match(
                    some: existing =>
                    {
                        _characterInventoryRepository.ItemInventory.Remove(existing);
                        if (item.Amount > 0 || item.Id == 1)
                        {
                            _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(existing.Amount + (item.Amount * (addingItemFromInventory ? -1 : 1))));
                        }
                    },
                    none: () =>
                    {
                        if (item.Amount > 0)
                            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(item.Id, item.Amount));
                    });

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, weight.Max);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
        }
    }
}
