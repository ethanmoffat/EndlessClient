// Some of this work is reverse-engineered from 
//     EOHAX C# DLLs written by Sausage (www.tehsausage.com)
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.IO;
using EOLib.IO.Services;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType]
    public sealed class PacketEncoderService : IPacketEncoderService
    {
        private readonly INumberEncoderService _numberEncoderService;
        private readonly IDataEncoderService _dataEncoderService;

        public PacketEncoderService(INumberEncoderService numberEncoderService,
                                    IDataEncoderService dataEncoderService)
        {
            _numberEncoderService = numberEncoderService;
            _dataEncoderService = dataEncoderService;
        }

        public byte[] PrependLengthBytes(byte[] data)
        {
            var ret = PrependLength(data.ToList());
            return ret.ToArray();
        }

        public IPacket AddSequenceNumber(IPacket pkt, int sequenceNumber)
        {
            var byteList = pkt.RawData;
            byteList = AddSequenceBytes(byteList, sequenceNumber);
            return new Packet(byteList.ToList());
        }

        public byte[] Encode(IPacket original, byte encodeMultiplier)
        {
            if (encodeMultiplier == 0 || !PacketValidForEncode(original))
                return original.RawData.ToArray();

            var byteList = original.RawData.ToList();
            byteList = _dataEncoderService.SwapMultiples(byteList, encodeMultiplier);
            byteList = _dataEncoderService.Interleave(byteList);
            byteList = _dataEncoderService.FlipMSB(byteList);

            return byteList.ToArray();
        }

        public IPacket Decode(IEnumerable<byte> original, byte decodeMultiplier)
        {
            var originalBytes = original.ToArray();
            if (decodeMultiplier == 0 || !PacketValidForDecode(originalBytes))
                return new Packet(originalBytes);

            var byteList = originalBytes.ToList();
            byteList = _dataEncoderService.FlipMSB(byteList);
            byteList = _dataEncoderService.Deinterleave(byteList);
            byteList = _dataEncoderService.SwapMultiples(byteList, decodeMultiplier);

            return new Packet(byteList);
        }

        #region Packet Validation

        private static bool PacketValidForEncode(IPacket pkt)
        {
            return !IsInitPacket(pkt);
        }

        private static bool PacketValidForDecode(byte[] data)
        {
            return data.Length >= 2 && !IsInitPacket(new Packet(new[] {data[0], data[1]}));
        }

        private static bool IsInitPacket(IPacket pkt)
        {
            return pkt.Family == PacketFamily.Init &&
                   pkt.Action == PacketAction.Init;
        }

        #endregion

        #region Sequence Byte(s)

        private List<byte> AddSequenceBytes(IReadOnlyList<byte> original, int seq)
        {
            var numberOfSequenceBytes = seq >= NumericConstants.ONE_BYTE_MAX ? 2 : 1;
            var encodedSequenceBytes = _numberEncoderService.EncodeNumber(seq, numberOfSequenceBytes);

            var combined = new List<byte>(original.Count + numberOfSequenceBytes);
            //family/action copied to [0][1]
            combined.AddRange(new[] {original[0], original[1]});
            //sequence number copied to [2] (and [3] if it's a two-byte number)
            combined.AddRange(encodedSequenceBytes);
            //add the remaining data - rest of data copied to [3] (or [4]) onward [...]
            combined.AddRange(original.Where((b, i) => i >= 2));

            return combined;
        }

        #endregion

        #region Length Bytes

        private List<byte> PrependLength(IReadOnlyList<byte> data)
        {
            var len = _numberEncoderService.EncodeNumber(data.Count, 2);
            var combined = new List<byte>(data.Count + len.Length);

            combined.AddRange(len);
            combined.AddRange(data);

            return combined;
        }

        #endregion
    }

    public interface IPacketEncoderService
    {
        byte[] PrependLengthBytes(byte[] data);

        IPacket AddSequenceNumber(IPacket pkt, int sequenceNumber);

        byte[] Encode(IPacket original, byte encodeMultiplier);

        IPacket Decode(IEnumerable<byte> original, byte decodeMultiplier);
    }
}
