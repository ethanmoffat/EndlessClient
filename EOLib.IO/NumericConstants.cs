// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;

namespace EOLib.IO
{
    [ExcludeFromCodeCoverage]
    public static class NumericConstants
    {
        public const int ONE_BYTE_MAX = 253;
        public const int TWO_BYTE_MAX = 64009;
        public const int THREE_BYTE_MAX = 16194277;

        public static readonly int[] NUMERIC_MAXIMUM = {ONE_BYTE_MAX, TWO_BYTE_MAX, THREE_BYTE_MAX };
    }
}
