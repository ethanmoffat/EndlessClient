using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildReportHandler : InGameOnlyPacketHandler<GuildReportServerPacket>
    {
        private readonly IGuildSessionRepository _guildSessionRepository;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Report;

        public GuildReportHandler(IPlayerInfoProvider playerInfoProvider,
                                  IGuildSessionRepository guildSessionRepository)
            : base(playerInfoProvider)
        {
            _guildSessionRepository = guildSessionRepository;
        }

        public override bool HandlePacket(GuildReportServerPacket packet)
        {
            _guildSessionRepository.GuildInfo = Option.Some(GuildInfo.FromPacket(packet));
            return true;
        }
    }
}
