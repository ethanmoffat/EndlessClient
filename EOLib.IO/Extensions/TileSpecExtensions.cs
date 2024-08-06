using EOLib.IO.Map;

namespace EOLib.IO.Extensions
{
    public static class TileSpecExtensions
    {
        public static bool IsBoard(this TileSpec tileSpec)
        {
            return tileSpec >= (TileSpec)20 && tileSpec <= (TileSpec)27;
        }

        public static bool IsChair(this TileSpec tileSpec)
        {
            return tileSpec >= (TileSpec)1 && tileSpec <= (TileSpec)7;
        }
    }
}
