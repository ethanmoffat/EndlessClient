using EOLib.IO.Map;

namespace EOLib.IO.Extensions
{
    public static class TileSpecExtensions
    {
        public static bool IsBoard(this TileSpec tileSpec)
        {
            return tileSpec == TileSpec.Board1 ||
                tileSpec == TileSpec.Board2 ||
                tileSpec == TileSpec.Board3 ||
                tileSpec == TileSpec.Board4 ||
                tileSpec == TileSpec.Board5 ||
                tileSpec == TileSpec.Board6 ||
                tileSpec == TileSpec.Board7 ||
                tileSpec == TileSpec.Board8;
        }
    }
}
