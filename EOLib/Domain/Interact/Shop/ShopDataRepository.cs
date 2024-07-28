using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Domain.Interact.Shop
{
    public interface IShopDataRepository : IResettable
    {
        int SessionID { get; set; }

        string ShopName { get; set; }

        List<IShopItem> TradeItems { get; set; }

        List<IShopCraftItem> CraftItems { get; set; }
    }

    public interface IShopDataProvider : IResettable
    {
        int SessionID { get; }

        string ShopName { get; }

        IReadOnlyList<IShopItem> TradeItems { get; }

        IReadOnlyList<IShopCraftItem> CraftItems { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class ShopDataRepository : IShopDataProvider, IShopDataRepository
    {
        public int SessionID { get; set; }
        public string ShopName { get; set; }

        public List<IShopItem> TradeItems { get; set; }

        public List<IShopCraftItem> CraftItems { get; set; }

        IReadOnlyList<IShopItem> IShopDataProvider.TradeItems => TradeItems;

        IReadOnlyList<IShopCraftItem> IShopDataProvider.CraftItems => CraftItems;

        public ShopDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            SessionID = 0;
            ShopName = string.Empty;
            TradeItems = new List<IShopItem>();
            CraftItems = new List<IShopCraftItem>();
        }
    }
}