// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net.PacketProcessing
{
    public class PacketSequenceService : IPacketSequenceService
    {
        public int CalculateInitialSequenceNumber(int seq1, int seq2)
        {
            return seq1*7 - 11 + seq2 - 2;
        }

        public int CalculateNextSequenceNumber(int sequence, int increment)
        {
            return sequence + increment;
        }

        public int CalculateNewInitialSequenceNumber(int seq1, int seq2)
        {
            return seq1 - seq2;
        }

        public int CalculateNextSequenceIncrement(int increment)
        {
            return (increment + 1)%10;
        }
    }
}
