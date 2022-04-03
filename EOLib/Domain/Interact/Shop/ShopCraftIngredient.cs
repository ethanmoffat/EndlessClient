namespace EOLib.Domain.Interact.Shop
{
    public class ShopCraftIngredient : IShopCraftIngredient
    {
        public int ID { get; }

        public int Amount { get; }

        public ShopCraftIngredient(int id, int amount)
        {
            ID = id;
            Amount = amount;
        }
    }

    public interface IShopCraftIngredient
    {
        int ID { get; }

        int Amount { get; }
    }
}
