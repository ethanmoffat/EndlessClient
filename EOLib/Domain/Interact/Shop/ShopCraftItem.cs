using System.Collections.Generic;

namespace EOLib.Domain.Interact.Shop
{
    public class ShopCraftItem : IShopCraftItem
    {
        public int ID { get; }

        public IReadOnlyList<IShopCraftIngredient> Ingredients { get; }

        public ShopCraftItem(int id, IEnumerable<IShopCraftIngredient> ingredients)
        {
            ID = id;
            Ingredients = new List<IShopCraftIngredient>(ingredients);
        }
    }

    public interface IShopCraftItem
    {
        int ID { get; }

        IReadOnlyList<IShopCraftIngredient> Ingredients { get; }
    }
}
