// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using System;
using System.Text;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(IMapStringEncoderService))]
    public class MapStringEncoderService : IMapStringEncoderService
    {
        public string DecodeMapString(byte[] chars)
        {
            Array.Reverse(chars);

            bool flippy = chars.Length % 2 == 1;

            for (int i = 0; i < chars.Length; ++i)
            {
                byte c = chars[i];
                if (c == 0xFF)
                {
                    Array.Resize(ref chars, i);
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

                chars[i] = c;
                flippy = !flippy;
            }

            return Encoding.ASCII.GetString(chars);
        }

        public byte[] EncodeMapString(string s)
        {
            byte[] chars = Encoding.ASCII.GetBytes(s);
            bool flippy = chars.Length % 2 == 1;
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
            return chars;
        }
    }
}
