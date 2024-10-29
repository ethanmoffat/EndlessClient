using System.Collections.Generic;
using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Notifiers
{
    public interface IGuildNotifier
    {
        void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity);

        void NotifyRequestToJoinGuild(int playerId, string name);

        void NotifyGuildReply(GuildReply reply);

        void NotifyConfirmCreateGuild();

        void NotifyNewGuildBankBalance(int balance);

        void NotifyAcceptedIntoGuild();

        void NotifyRanks(IReadOnlyList<string> ranks);
    }

    [AutoMappedType]
    public class NoOpGuildNotifier : IGuildNotifier
    {
        public void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity) { }
        public void NotifyRequestToJoinGuild(int playerId, string name) { }
        public void NotifyGuildReply(GuildReply reply) { }
        public void NotifyConfirmCreateGuild() { }
        public void NotifyNewGuildBankBalance(int balance) { }
        public void NotifyAcceptedIntoGuild() { }
        public void NotifyRanks(IReadOnlyList<string> ranks) { }
    }
}
