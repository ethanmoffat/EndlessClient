using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Packet;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType(IsSingleton = true)]
    public class SequenceRepository : ISequenceRepository
    {
        public PacketSequencer Sequencer { get; set; }

        public SequenceRepository() => ResetState();

        public void ResetState()
        {
            Sequencer = new PacketSequencer(ZeroSequence.Instance);
        }
    }

    public interface ISequenceRepository : IResettable
    {
        PacketSequencer Sequencer { get; set; }
    }
}