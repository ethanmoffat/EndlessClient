using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Notifiers
{
    public interface IGuildNotifier
    {
        void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity);

        void NotifyRequestToJoinGuild(int playerId, string name);

        void NotifyGuildReply(GuildReply reply);
    }

    [AutoMappedType]
    public class NoOpGuildNotifier : IGuildNotifier
    {
        public void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity) { }
        public void NotifyRequestToJoinGuild(int playerId, string name) { }
        public void NotifyGuildReply(GuildReply reply) { }

        public void NotifyRecruiterOffline() { }
        public void NotifyRecruiterNotHere() { }
        public void NotifyRecruiterWrongGuild() { }
        public void NotifyNotRecruiter() { }
        public void NotifyConfirmCreateGuild() { }
        public void NotifyNotApproved() { }
        public void NotifyExists() { }
        public void NotifyNoCandidates() { }
        public void NotifyBusy() { }
    }
}
