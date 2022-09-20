using System.Diagnostics.CodeAnalysis;

namespace EOLib.IO
{
    [ExcludeFromCodeCoverage]
    public static class NumericConstants
    {
        public const int ONE_BYTE_MAX = 253;
        public const int TWO_BYTE_MAX = ONE_BYTE_MAX * ONE_BYTE_MAX;
        public const int THREE_BYTE_MAX = ONE_BYTE_MAX * ONE_BYTE_MAX * ONE_BYTE_MAX;

        public static readonly int[] NUMERIC_MAXIMUM = {ONE_BYTE_MAX, TWO_BYTE_MAX, THREE_BYTE_MAX };
    }
}
