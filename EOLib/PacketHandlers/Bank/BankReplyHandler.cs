using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Bank;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Bank
{
    [AutoMappedType]
    public class BankReplyHandler : InGameOnlyPacketHandler
    {
        private readonly IBankDataRepository _bankDataRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Bank;

        public override PacketAction Action => PacketAction.Reply;

        public BankReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                IBankDataRepository bankDataRepository,
                                ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _bankDataRepository = bankDataRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var characterGold = packet.ReadInt();
            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, characterGold));

            _bankDataRepository.AccountValue = packet.ReadInt();

            return true;
        }
    }
}
