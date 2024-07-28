using AutomaticTypeMapper;
using System;
using System.Linq;
using System.Text;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IMapStringEncoderService))]
    public class MapStringEncoderService : IMapStringEncoderService
    {
        public string DecodeMapString(byte[] chars)
        {
            var copy = new byte[chars.Length];
            Array.Copy(chars, copy, chars.Length);
            Array.Reverse(copy);

            bool flippy = copy.Length % 2 == 1;

            for (int i = 0; i < copy.Length; ++i)
            {
                byte c = copy[i];
                if (c == 0xFF)
                {
                    Array.Resize(ref copy, i);
                    break;
                }

                if (flippy)
                {
                    if (c >= 0x22 && c <= 0x4F)
                        c = (byte)(0x71 - c);
                    else if (c >= 0x50 && c <= 0x7E)
                        c = (byte)(0xCD - c);
                }
                else
                {
                    if (c >= 0x22 && c <= 0x7E)
                        c = (byte)(0x9F - c);
                }

                copy[i] = c;
                flippy = !flippy;
            }

            return Encoding.ASCII.GetString(copy);
        }

        public byte[] EncodeMapString(string s, int length)
        {
            if (length < s.Length)
                throw new ArgumentException("Length should be greater than or equal to string length", nameof(length));

            byte[] chars = Encoding.ASCII.GetBytes(s);
            bool flippy = length % 2 == 1;
            int i;
            for (i = 0; i < chars.Length; ++i)
            {
                byte c = chars[i];
                if (flippy)
                {
                    if (c >= 0x22 && c <= 0x4F)
                        c = (byte)(0x71 - c);
                    else if (c >= 0x50 && c <= 0x7E)
                        c = (byte)(0xCD - c);
                }
                else
                {
                    if (c >= 0x22 && c <= 0x7E)
                        c = (byte)(0x9F - c);
                }
                chars[i] = c;
                flippy = !flippy;
            }
            Array.Reverse(chars);

            if (length > s.Length)
            {
                var tmp = Enumerable.Repeat((byte)0xFF, length).ToArray();
                chars.CopyTo(tmp, length - s.Length);
                chars = tmp;
            }

            return chars;
        }
    }
}