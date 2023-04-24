using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact.Shop
{
    [AutoMappedType]
    public class ShopActions : IShopActions
    {
        private readonly IPacketSendService _packetSendService;

        public ShopActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void BuyItem(int itemId, int amount)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Buy)
                .AddShort(itemId)
                .AddInt(amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void SellItem(int itemId, int amount)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Sell)
                .AddShort(itemId)
                .AddInt(amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void CraftItem(int itemId)
        {
            var packet = new PacketBuilder(PacketFamily.Shop, PacketAction.Create)
                .AddShort(itemId)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IShopActions
    {
        void BuyItem(int itemId, int amount);

        void SellItem(int itemId, int amount);

        void CraftItem(int itemId);
    }
}
