using System;

namespace EOLib.IO.Extensions
{
    public static class ItemSizeExtensions
    {
        public static (int Width, int Height) GetDimensions(this ItemSize itemSize)
        {
            var sizeStr = Enum.GetName(typeof(ItemSize), itemSize);
            if (sizeStr == null || sizeStr.Length != 7)
            {
                return (0, 0);
            }

            var width = Convert.ToInt32(sizeStr.Substring(4, 1));
            var height = Convert.ToInt32(sizeStr.Substring(6, 1));

            return (width, height);
        }
    }
}
