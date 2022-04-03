using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact.Shop
{
    public class ShopActions : IShopActions
    {
        private readonly IPacketSendService _packetSendService;

        public ShopActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void BuyItem(short itemId, int amount)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Buy)
                .AddShort(itemId)
                .AddInt(amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void SellItem(short itemId, int amount)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Sell)
                .AddShort(itemId)
                .AddInt(amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void CraftItem(short itemId)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Create)
                .AddShort(itemId)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IShopActions
    {
        void BuyItem(short itemId, int amount);

        void SellItem(short itemId, int amount);

        void CraftItem(short itemId);
    }
}
