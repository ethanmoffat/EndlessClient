using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildTakeHandler : InGameOnlyPacketHandler<GuildTakeServerPacket>
    {
        private readonly IGuildSessionRepository _guildSessionRepository;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Take;

        public GuildTakeHandler(IPlayerInfoProvider playerInfoProvider,
                                 IGuildSessionRepository guildSessionRepository)
            : base(playerInfoProvider)
        {
            _guildSessionRepository = guildSessionRepository;
        }

        public override bool HandlePacket(GuildTakeServerPacket packet)
        {
            _guildSessionRepository.GuildDescription = packet.Description;

            return true;
        }
    }
}
