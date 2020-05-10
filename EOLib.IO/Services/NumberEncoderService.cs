using AutomaticTypeMapper;
using System.Linq;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(INumberEncoderService))]
    public class NumberEncoderService : INumberEncoderService
    {
        public byte[] EncodeNumber(int number, int size)
        {
            var unsigned = (uint) number;
            var numArray = Enumerable.Repeat((uint)254, 4).ToArray();
            var original = unsigned;

            if (original >= NumericConstants.THREE_BYTE_MAX)
            {
                numArray[3] = unsigned / NumericConstants.THREE_BYTE_MAX + 1;
                unsigned = unsigned % NumericConstants.THREE_BYTE_MAX;
            }

            if (original >= NumericConstants.TWO_BYTE_MAX)
            {
                numArray[2] = unsigned / NumericConstants.TWO_BYTE_MAX + 1;
                unsigned = unsigned % NumericConstants.TWO_BYTE_MAX;
            }

            if (original >= NumericConstants.ONE_BYTE_MAX)
            {
                numArray[1] = unsigned / NumericConstants.ONE_BYTE_MAX + 1;
                unsigned = unsigned % NumericConstants.ONE_BYTE_MAX;
            }

            numArray[0] = unsigned + 1;

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
