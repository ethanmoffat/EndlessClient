using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IJukeboxNotifier
    {
        void JukeboxUnavailable();
    }

    [AutoMappedType]
    public class NoOpJukeboxNotifier : IJukeboxNotifier
    {
        public void JukeboxUnavailable() { }
    }

}
