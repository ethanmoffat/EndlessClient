using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IGuildNotifier
    {
        void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity);
        void NotifyRequestToJoinGuild(int playerId, string name);
        void NotifyGuildDetailsUpdated();
    }

    [AutoMappedType]
    public class NoOpGuildNotifier : IGuildNotifier
    {
        public void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity) { }
        public void NotifyRequestToJoinGuild(int playerId, string name) { }
        public void NotifyGuildDetailsUpdated() { }
    }
}
