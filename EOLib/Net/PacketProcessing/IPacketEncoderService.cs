// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.Net.PacketProcessing
{
    public interface IPacketEncoderService
    {
        byte[] PrependLengthBytes(byte[] data);

        IPacket AddSequenceNumber(IPacket pkt, int sequenceNumber);

        byte[] Encode(IPacket original, byte encodeMultiplier);

        IPacket Decode(IEnumerable<byte> original, byte decodeMultiplier);
    }
}
