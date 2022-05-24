using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Bank;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Locker
{
    [AutoMappedType]
    public class LockerBuyHandler : InGameOnlyPacketHandler
    {
        private const byte BuySellSfxId = 26;

        private readonly IBankDataRepository _bankDataRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.Locker;

        public override PacketAction Action => PacketAction.Buy;

        public LockerBuyHandler(IPlayerInfoProvider playerInfoProvider,
                                IBankDataRepository bankDataRepository,
                                ICharacterInventoryRepository characterInventoryRepository,
                                IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _bankDataRepository = bankDataRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var inventoryGold = packet.ReadInt();
            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, inventoryGold));

            _bankDataRepository.LockerUpgrades = Option.Some<int>(packet.ReadChar());

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(BuySellSfxId);

            return true;
        }
    }
}
