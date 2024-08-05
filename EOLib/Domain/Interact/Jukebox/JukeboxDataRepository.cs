using AutomaticTypeMapper;
using EOLib.Domain.Map;
using Optional;

namespace EOLib.Domain.Interact.Jukebox
{
    public interface IJukeboxRepository
    {
        Option<string> PlayingRequestName { get; set; }
    }

    public interface IJukeboxProvider
    {
        Option<string> PlayingRequestName { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class JukeboxDataRepository : IJukeboxRepository, IJukeboxProvider, IResettable
    {
        public Option<string> PlayingRequestName { get; set; }

        public JukeboxDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            PlayingRequestName = Option.None<string>();
        }
    }
}