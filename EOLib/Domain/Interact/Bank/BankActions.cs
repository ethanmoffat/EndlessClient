using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact.Bank
{
    [AutoMappedType]
    public class BankActions : IBankActions
    {
        private readonly IPacketSendService _packetSendService;

        public BankActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void Deposit(int amount)
        {
            var packet = new PacketBuilder(PacketFamily.Bank, PacketAction.Add)
                .AddInt(amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void Withdraw(int amount)
        {
            var packet = new PacketBuilder(PacketFamily.Bank, PacketAction.Take)
                .AddInt(amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void BuyStorageUpgrade()
        {
            var packet = new PacketBuilder(PacketFamily.Locker, PacketAction.Buy)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IBankActions
    {
        void Deposit(int amount);

        void Withdraw(int amount);

        void BuyStorageUpgrade();
    }
}
