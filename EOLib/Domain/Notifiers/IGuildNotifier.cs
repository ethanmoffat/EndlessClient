using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface IGuildNotifier
    {
        void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity);
        void NotifyRequestToJoinGuild(int playerId, string name);
        void NotifyRecruiterOffline();
        void NotifyRecruiterNotHere();
        void NotifyRecruiterWrongGuild();
        void NotifyNotRecruiter();
    }

    [AutoMappedType]
    public class NoOpGuildNotifier : IGuildNotifier
    {
        public void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity) { }
        public void NotifyRequestToJoinGuild(int playerId, string name) { }
        public void NotifyRecruiterOffline() { }
        public void NotifyRecruiterNotHere() { }
        public void NotifyRecruiterWrongGuild() { }
        public void NotifyNotRecruiter() { }
    }
}
