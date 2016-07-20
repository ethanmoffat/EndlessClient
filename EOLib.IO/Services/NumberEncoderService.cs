// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;

namespace EOLib.IO.Services
{
    public class NumberEncoderService : INumberEncoderService
    {
        public byte[] EncodeNumber(int number, int size)
        {
            var numArray = Enumerable.Repeat(254, 4).ToArray();
            var original = number;

            if (original >= NumericConstants.THREE_BYTE_MAX)
            {
                numArray[3] = number/NumericConstants.THREE_BYTE_MAX + 1;
                number = number%NumericConstants.THREE_BYTE_MAX;
            }

            if (original >= NumericConstants.TWO_BYTE_MAX)
            {
                numArray[2] = number/NumericConstants.TWO_BYTE_MAX + 1;
                number = number%NumericConstants.TWO_BYTE_MAX;
            }

            if (original >= NumericConstants.ONE_BYTE_MAX)
            {
                numArray[1] = number/NumericConstants.ONE_BYTE_MAX + 1;
                number = number%NumericConstants.ONE_BYTE_MAX;
            }

            numArray[0] = number + 1;

            return numArray.Select(x => (byte)x)
                           .Take(size)
                           .ToArray();
        }

        public int DecodeNumber(params byte[] b)
        {
            for (int index = 0; index < b.Length; ++index)
            {
                if (b[index] == 254)
                    b[index] = 1;
                else if (b[index] == 0)
                    b[index] = 128;
                --b[index];
            }

            var retNum = 0;
            if (b.Length > 3)
                retNum += b[3]*NumericConstants.THREE_BYTE_MAX;
            if (b.Length > 2)
                retNum += b[2]*NumericConstants.TWO_BYTE_MAX;
            if (b.Length > 1)
                retNum += b[1]*NumericConstants.ONE_BYTE_MAX;

            return retNum + b[0];
        }
    }
}
