using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Interact.Guild
{
    [AutoMappedType]
    public class GuildActions : IGuildActions
    {
        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly IPacketSendService _packetSendService;

        public GuildActions(IGuildSessionProvider guildSessionProvider,
                          IPacketSendService packetSendService)
        {
            _guildSessionProvider = guildSessionProvider;
            _packetSendService = packetSendService;
        }

        public void Lookup(string identity)
        {
            _packetSendService.SendPacket(new GuildReportClientPacket { SessionId = _guildSessionProvider.SessionID, GuildIdentity = identity });
        }

        public void ViewMembers(string identity)
        {
            _packetSendService.SendPacket(new GuildTellClientPacket { SessionId = _guildSessionProvider.SessionID, GuildIdentity = identity });
        }

        public void RequestToJoinGuild(string guildTag, string recruiterName)
        {
            _packetSendService.SendPacket(new GuildPlayerClientPacket { SessionId = _guildSessionProvider.SessionID, GuildTag = guildTag, RecruiterName = recruiterName });
        }
    }

    public interface IGuildActions
    {
        void Lookup(string identity);
        void ViewMembers(string identity);
        void RequestToJoinGuild(string guildTag, string recruiterName);
    }
}
