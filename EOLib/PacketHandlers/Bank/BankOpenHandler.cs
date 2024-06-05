using AutomaticTypeMapper;
using EOLib.Domain.Interact.Bank;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Bank
{
    [AutoMappedType]
    public class BankOpenHandler : InGameOnlyPacketHandler<BankOpenServerPacket>
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

        public override bool HandlePacket(BankOpenServerPacket packet)
        {
            _bankDataRepository.AccountValue = packet.GoldBank;
            _bankDataRepository.SessionID = packet.SessionId;
            _bankDataRepository.LockerUpgrades = Option.Some(packet.LockerUpgrades);

            return true;
        }
    }
}
