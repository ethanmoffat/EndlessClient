using AutomaticTypeMapper;
using EOLib.Domain.Interact.Bank;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;

namespace EOLib.PacketHandlers.Bank
{
    [AutoMappedType]
    public class BankOpenHandler : InGameOnlyPacketHandler
    {
        private readonly IBankDataRepository _bankDataRepository;

        public override PacketFamily Family => PacketFamily.Bank;

        public override PacketAction Action => PacketAction.Open;

        public BankOpenHandler(IPlayerInfoProvider playerInfoProvider,
                               IBankDataRepository bankDataRepository)
            : base(playerInfoProvider)
        {
            _bankDataRepository = bankDataRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            _bankDataRepository.AccountValue = packet.ReadInt();
            _bankDataRepository.SessionID = packet.ReadThree();
            _bankDataRepository.LockerUpgrades = Option.Some<int>(packet.ReadChar());

            return true;
        }
    }
}
