using AutomaticTypeMapper;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD
{
    [AutoMappedType]
    public class ItemNameColorService : IItemNameColorService
    {
        public Color GetColorForInventoryDisplay(EIFRecord itemData) => GetColor(itemData, ColorConstants.LightGrayText);

        public Color GetColorForMapDisplay(EIFRecord itemData) => GetColor(itemData, Color.White);

        private static Color GetColor(EIFRecord itemData, Color defaultColor)
        {
            switch (itemData.Special)
            {
                case ItemSpecial.Lore:
                case ItemSpecial.Unique:
                    return Color.FromNonPremultiplied(0xff, 0xf0, 0xa5, 0xff);
                case ItemSpecial.Rare:
                    return Color.FromNonPremultiplied(0xf5, 0xc8, 0x9c, 0xff);
            }

            return defaultColor;
        }
    }

    public interface IItemNameColorService
    {
        Color GetColorForMapDisplay(EIFRecord itemData);

        Color GetColorForInventoryDisplay(EIFRecord itemData);
    }
}