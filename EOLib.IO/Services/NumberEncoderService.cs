// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Services
{
    public class NumberEncoderService : INumberEncoderService
    {
        public byte[] EncodeNumber(int number, int size)
        {
            byte[] numArray = new byte[size];
            for (int index = 3; index >= 1; --index)
            {
                if (index >= numArray.Length)
                {
                    if (number >= NumericConstants.NUMERIC_MAXIMUM[index - 1])
                        number %= NumericConstants.NUMERIC_MAXIMUM[index - 1];
                }
                else if (number >= NumericConstants.NUMERIC_MAXIMUM[index - 1])
                {
                    numArray[index] = (byte)(number / NumericConstants.NUMERIC_MAXIMUM[index - 1] + 1);
                    number %= NumericConstants.NUMERIC_MAXIMUM[index - 1];
                }
                else
                    numArray[index] = 254;
            }
            numArray[0] = (byte)(number + 1);
            return numArray;
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

            int num = 0;
            for (int index = b.Length - 1; index >= 1; --index)
                num += b[index] * NumericConstants.NUMERIC_MAXIMUM[index - 1];
            return num + b[0];
        }
    }
}
