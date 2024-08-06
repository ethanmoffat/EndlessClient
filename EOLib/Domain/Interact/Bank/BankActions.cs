using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Interact.Bank
{
    [AutoMappedType]
    public class BankActions : IBankActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IBankDataProvider _bankDataProvider;

        public BankActions(IPacketSendService packetSendService,
                           IBankDataProvider bankDataProvider)
        {
            _packetSendService = packetSendService;
            _bankDataProvider = bankDataProvider;
        }

        public void Deposit(int amount)
        {
            var packet = new BankAddClientPacket { Amount = amount, SessionId = _bankDataProvider.SessionID };
            _packetSendService.SendPacket(packet);
        }

        public void Withdraw(int amount)
        {
            var packet = new BankTakeClientPacket { Amount = amount, SessionId = _bankDataProvider.SessionID };
            _packetSendService.SendPacket(packet);
        }

        public void BuyStorageUpgrade() => _packetSendService.SendPacket(new LockerBuyClientPacket());
    }

    public interface IBankActions
    {
        void Deposit(int amount);

        void Withdraw(int amount);

        void BuyStorageUpgrade();
    }
}
