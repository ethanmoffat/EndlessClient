using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Bank;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Bank
{
    [AutoMappedType]
    public class BankReplyHandler : InGameOnlyPacketHandler<BankReplyServerPacket>
    {
        private const byte BuySellSfxId = 26;

        private readonly IBankDataRepository _bankDataRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.Bank;

        public override PacketAction Action => PacketAction.Reply;

        public BankReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                IBankDataRepository bankDataRepository,
                                ICharacterInventoryRepository characterInventoryRepository,
                                IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _bankDataRepository = bankDataRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(BankReplyServerPacket packet)
        {
            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, packet.GoldInventory));

            _bankDataRepository.AccountValue = packet.GoldBank;

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(BuySellSfxId);

            return true;
        }
    }
}
