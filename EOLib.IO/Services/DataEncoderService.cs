using AutomaticTypeMapper;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.IO.Services
{
    [AutoMappedType]
    public class DataEncoderService : IDataEncoderService
    {
        public List<byte> Interleave(IReadOnlyList<byte> data)
        {
            var numArray = new byte[data.Count];
            var index1 = 0;
            var num = 0;

            while (index1 < data.Count)
            {
                numArray[index1] = data[num++];
                index1 += 2;
            }

            var index2 = index1 - 1;
            if (data.Count % 2 != 0)
                index2 -= 2;

            while (index2 >= 0)
            {
                numArray[index2] = data[num++];
                index2 -= 2;
            }

            return numArray.ToList();
        }

        public List<byte> Deinterleave(IReadOnlyList<byte> data)
        {
            var numArray = new byte[data.Count];
            var index1 = 0;
            var num = 0;

            while (index1 < data.Count)
            {
                numArray[num++] = data[index1];
                index1 += 2;
            }

            var index2 = index1 - 1;
            if (data.Count % 2 != 0)
                index2 -= 2;

            while (index2 >= 0)
            {
                numArray[num++] = data[index2];
                index2 -= 2;
            }

            return numArray.ToList();
        }

        public List<byte> FlipMSB(IReadOnlyList<byte> data)
        {
            return data.Select(x => (byte)(x == 128 || x == 0 ? x : x ^ 0x80u)).ToList();
        }

        public List<byte> SwapMultiples(IReadOnlyList<byte> data, int multi)
        {
            int num1 = 0;

            var result = data.ToArray();

            for (int index1 = 0; index1 <= data.Count; ++index1)
            {
                if (index1 != data.Count && data[index1] % multi == 0)
                {
                    ++num1;
                }
                else
                {
                    if (num1 > 1)
                    {
                        for (int index2 = 0; index2 < num1 / 2; ++index2)
                        {
                            byte num2 = data[index1 - num1 + index2];
                            result[index1 - num1 + index2] = data[index1 - index2 - 1];
                            result[index1 - index2 - 1] = num2;
                        }
                    }
                    num1 = 0;
                }
            }

            return result.ToList();
        }
    }

    public interface IDataEncoderService
    {
        List<byte> Interleave(IReadOnlyList<byte> data);

        List<byte> Deinterleave(IReadOnlyList<byte> data);

        List<byte> FlipMSB(IReadOnlyList<byte> data);

        List<byte> SwapMultiples(IReadOnlyList<byte> data, int multi);
    }
}
