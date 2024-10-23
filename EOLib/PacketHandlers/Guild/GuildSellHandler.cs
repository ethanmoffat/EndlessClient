using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildSellHandler : InGameOnlyPacketHandler<GuildSellServerPacket>
    {
        private readonly IGuildSessionRepository _guildSessionRepository;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Sell;

        public GuildSellHandler(IPlayerInfoProvider playerInfoProvider,
                                 IGuildSessionRepository guildSessionRepository)
            : base(playerInfoProvider)
        {
            _guildSessionRepository = guildSessionRepository;
        }

        public override bool HandlePacket(GuildSellServerPacket packet)
        {
            _guildSessionRepository.GuildBalance = packet.GoldAmount;
            return true;
        }
    }
}
