using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Interact.Shop
{
    [AutoMappedType]
    public class ShopActions : IShopActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IShopDataProvider _shopDataProvider;

        public ShopActions(IPacketSendService packetSendService,
                           IShopDataProvider shopDataProvider)
        {
            _packetSendService = packetSendService;
            _shopDataProvider = shopDataProvider;
        }

        public void BuyItem(int itemId, int amount)
        {
            var packet = new ShopBuyClientPacket
            {
                SessionId = _shopDataProvider.SessionID,
                BuyItem = new Moffat.EndlessOnline.SDK.Protocol.Net.Item
                {
                    Id = itemId,
                    Amount = amount
                }
            };
            _packetSendService.SendPacket(packet);
        }

        public void SellItem(int itemId, int amount)
        {
            var packet = new ShopSellClientPacket
            {
                SessionId = _shopDataProvider.SessionID,
                SellItem = new Moffat.EndlessOnline.SDK.Protocol.Net.Item
                {
                    Id = itemId,
                    Amount = amount
                }
            };
            _packetSendService.SendPacket(packet);
        }

        public void CraftItem(int itemId)
        {
            var packet = new ShopCreateClientPacket
            {
                SessionId = _shopDataProvider.SessionID,
                CraftItemId = itemId
            };
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