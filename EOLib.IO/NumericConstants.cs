using System.Diagnostics.CodeAnalysis;

namespace EOLib.IO
{
    [ExcludeFromCodeCoverage]
    public static class NumericConstants
    {
        public const uint ONE_BYTE_MAX = 253;
        public const uint TWO_BYTE_MAX = ONE_BYTE_MAX * ONE_BYTE_MAX;
        public const uint THREE_BYTE_MAX = ONE_BYTE_MAX * ONE_BYTE_MAX * ONE_BYTE_MAX;
        public const uint FOUR_BYTE_MAX = ONE_BYTE_MAX * ONE_BYTE_MAX * ONE_BYTE_MAX * ONE_BYTE_MAX;

        public static readonly uint[] NUMERIC_MAXIMUM = { ONE_BYTE_MAX, TWO_BYTE_MAX, THREE_BYTE_MAX, FOUR_BYTE_MAX };
    }
}
