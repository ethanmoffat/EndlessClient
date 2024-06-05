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
    }

    public interface IGuildActions
    {
        void Lookup(string identity);

        void ViewMembers(string identity);
    }
}
