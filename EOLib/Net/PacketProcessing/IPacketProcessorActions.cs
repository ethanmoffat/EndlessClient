// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.Net.PacketProcessing
{
    public interface IPacketProcessorActions
    {
        void SetInitialSequenceNumber(int seq1, int seq2);

        void SetUpdatedBaseSequenceNumber(int seq1, int seq2);

        void SetEncodeMultiples(byte emulti_d, byte emulti_e);

        byte[] EncodePacket(OldPacket pkt);

        byte[] EncodePacket(IPacket pkt);
        
        byte[] EncodeRawPacket(IPacket pkt);

        OldPacket DecodeData(byte[] rawData);

        IPacket DecodeData(IEnumerable<byte> rawData);
    }
}
