using AutomaticTypeMapper;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType]
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

    public interface IPacketSequenceService
    {
        int CalculateInitialSequenceNumber(int seq1, int seq2);

        int CalculateNextSequenceNumber(int sequence, int increment);

        int CalculateNewInitialSequenceNumber(int seq1, int seq2);

        int CalculateNextSequenceIncrement(int increment);
    }
}
