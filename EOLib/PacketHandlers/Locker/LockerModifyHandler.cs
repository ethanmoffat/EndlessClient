using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Locker
{
    public abstract class LockerModifyHandler<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        private readonly ILockerDataRepository _lockerDataRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Locker;

        protected LockerModifyHandler(IPlayerInfoProvider playerInfoProvider,
                                      ILockerDataRepository lockerDataRepository,
                                      ICharacterRepository characterRepository,
                                      ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _lockerDataRepository = lockerDataRepository;
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        protected void Handle(int itemId, int amount, Weight weight, IReadOnlyList<ThreeItem> items)
        {
            _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == itemId)
                .Match(
                    some: existing =>
                    {
                        _characterInventoryRepository.ItemInventory.Remove(existing);
                        if (amount > 0 || itemId == 1)
                        {
                            _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(Action == PacketAction.Get ? existing.Amount + amount : amount));
                        }
                    },
                    none: () =>
                    {
                        if (amount > 0)
                            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(itemId, amount));
                    });

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, weight.Current)
                .WithNewStat(CharacterStat.MaxWeight, weight.Max);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            _lockerDataRepository.Items.Clear();
            foreach (var item in items)
                _lockerDataRepository.Items.Add(new InventoryItem(item.Id, item.Amount));
        }
    }
}