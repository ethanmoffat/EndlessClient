using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Data;
using System;

namespace EOLib.IO.Services
{
    [MappedType(BaseType = typeof(INumberEncoderService))]
    public class NumberEncoderService : INumberEncoderService
    {
        public byte[] EncodeNumber(int number, int size) => new ReadOnlySpan<byte>(NumberEncoder.EncodeNumber(number), 0, size).ToArray();

        public int DecodeNumber(params byte[] b) => NumberEncoder.DecodeNumber(b);
    }
}