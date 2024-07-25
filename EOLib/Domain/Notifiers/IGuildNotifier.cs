using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;

namespace EOLib.Domain.Notifiers
{
    public interface IGuildNotifier
    {
        void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity);
        void NotifyGuildDetailsUpdated();
    }

    [AutoMappedType]
    public class NoOpGuildNotifier : IGuildNotifier
    {
        public void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity) { }
        public void NotifyGuildDetailsUpdated() { }
    }
}