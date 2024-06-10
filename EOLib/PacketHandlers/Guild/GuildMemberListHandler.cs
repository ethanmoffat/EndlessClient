using System.Diagnostics;
using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]
    public class GuildMemberListHandler : InGameOnlyPacketHandler<GuildTellServerPacket>
    {
        private readonly IGuildSessionRepository _guildSessionRepository;
       
        public override PacketFamily Family => PacketFamily.Guild;
        public override PacketAction Action => PacketAction.Tell;

        public GuildMemberListHandler(IPlayerInfoProvider playerInfoProvider,
                                      IGuildSessionRepository guildSessionRepository)
            : base(playerInfoProvider)
        {
            _guildSessionRepository = guildSessionRepository;
        }

        public override bool HandlePacket(GuildTellServerPacket packet)
        {
            _guildSessionRepository.Members.Clear();

            foreach (var member in packet.Members)
            {
                _guildSessionRepository.Members[member.Name] = (member.Rank, member.RankName);
            }

            return true;
        }
    }
}
