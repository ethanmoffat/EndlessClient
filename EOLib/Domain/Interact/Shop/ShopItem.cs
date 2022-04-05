namespace EOLib.Domain.Interact.Shop
{
    public class ShopItem : IShopItem
    {
        public int ID { get; }

        public int Buy { get; }

        public int Sell { get; }

        public int MaxBuy { get; }

        public ShopItem(int id, int buyPrice, int sellPrice, int maxBuy)
        {
            ID = id;
            Buy = buyPrice;
            Sell = sellPrice;
            MaxBuy = maxBuy;
        }
    }

    public interface IShopItem
    {
        int ID { get; }

        int Buy { get; }

        int Sell { get; }

        int MaxBuy { get; }
    }
}
