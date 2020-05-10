using AutomaticTypeMapper;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType(IsSingleton = true)]
    public class SequenceRepository : ISequenceRepository
    {
        public int SequenceStart { get; set; }
        public int SequenceIncrement { get; set; }
    }

    public interface ISequenceRepository
    {
        int SequenceStart { get; set; }
        int SequenceIncrement { get; set; }
    }
}
