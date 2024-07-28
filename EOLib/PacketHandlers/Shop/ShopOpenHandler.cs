using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Shop;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Shop
{
    [AutoMappedType]
    public class ShopOpenHandler : InGameOnlyPacketHandler<ShopOpenServerPacket>
    {
        private readonly IShopDataRepository _shopDataRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Shop;

        public override PacketAction Action => PacketAction.Open;

        public ShopOpenHandler(IPlayerInfoProvider playerInfoProvider,
                               IShopDataRepository shopDataRepository,
                               IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _shopDataRepository = shopDataRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(ShopOpenServerPacket packet)
        {
            _shopDataRepository.SessionID = packet.SessionId;
            _shopDataRepository.ShopName = packet.ShopName;

            _shopDataRepository.TradeItems = packet.TradeItems
                .Where(x => x.ItemId > 0)
                .Select(x => new ShopItem(x.ItemId, x.BuyPrice, x.SellPrice, x.MaxBuyAmount))
                .ToList<IShopItem>();

            _shopDataRepository.CraftItems = packet.CraftItems
                .Where(x => x.ItemId > 0)
                .Select(x => new Domain.Interact.Shop.ShopCraftItem(
                    x.ItemId,
                    x.Ingredients.Where(x => x.Id > 0).Select(ing => new ShopCraftIngredient(ing.Id, ing.Amount)).ToList<IShopCraftIngredient>()))
                .ToList<IShopCraftItem>();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Shop);

            return true;
        }
    }
}