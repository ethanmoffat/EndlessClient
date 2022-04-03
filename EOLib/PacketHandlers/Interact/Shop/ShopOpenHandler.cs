using AutomaticTypeMapper;
using EOLib.Domain.Interact.Shop;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Interact.Shop
{
    [AutoMappedType]
    public class ShopOpenHandler : InGameOnlyPacketHandler
    {
        private readonly IShopDataRepository _shopDataRepository;

        public override PacketFamily Family => PacketFamily.Shop;

        public override PacketAction Action => PacketAction.Open;

        public ShopOpenHandler(IPlayerInfoProvider playerInfoProvider,
                               IShopDataRepository shopDataRepository)
            : base(playerInfoProvider)
        {
            _shopDataRepository = shopDataRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            _shopDataRepository.ShopID = packet.ReadShort();
            _shopDataRepository.ShopName = packet.ReadBreakString();

            var tradeItems = new List<IShopItem>();
            while (packet.PeekByte() != 255)
            {
                var nextItem = new ShopItem(
                    id: packet.ReadShort(),
                    buyPrice: packet.ReadThree(),
                    sellPrice: packet.ReadThree(),
                    maxBuy: packet.ReadChar());
                tradeItems.Add(nextItem);
            }
            packet.ReadByte();

            _shopDataRepository.TradeItems = tradeItems;

            var craftItems = new List<IShopCraftItem>();
            while (packet.PeekByte() != 255)
            {
                var id = packet.ReadShort();
                var ingreds = new List<IShopCraftIngredient>();

                for (int i = 0; i < 4; ++i)
                {
                    ingreds.Add(new ShopCraftIngredient(
                        id: packet.ReadShort(),
                        amount: packet.ReadChar()));
                }
                craftItems.Add(new ShopCraftItem(id, ingreds));
            }
            packet.ReadByte();

            _shopDataRepository.CraftItems = craftItems;

            return true;
        }
    }
}
