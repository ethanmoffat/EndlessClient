using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildTellHandler : InGameOnlyPacketHandler<GuildTellServerPacket>
    {
        private readonly IGuildSessionRepository _guildSessionRepository;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Tell;

        public GuildTellHandler(IPlayerInfoProvider playerInfoProvider,
                                IGuildSessionRepository guildSessionRepository)
            : base(playerInfoProvider)
        {
            _guildSessionRepository = guildSessionRepository;
        }

        public override bool HandlePacket(GuildTellServerPacket packet)
        {
            _guildSessionRepository.GuildMembers = new List<GuildMember>(packet.Members);
            return true;
        }
    }
}
