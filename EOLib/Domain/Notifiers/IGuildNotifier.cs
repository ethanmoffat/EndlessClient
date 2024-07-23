using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;

namespace EOLib.Domain.Notifiers
{
    public interface IGuildNotifier
    {
        void NotifyGuildCreationRequest(GuildCreationRequest request);
    }

    [AutoMappedType]
    public class NoOpGuildNotifier : IGuildNotifier
    {
        public void NotifyGuildCreationRequest(GuildCreationRequest request) { }
    }
}
