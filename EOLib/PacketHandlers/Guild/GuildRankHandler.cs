using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]
    public class GuildRankHandler : InGameOnlyPacketHandler<GuildRankServerPacket>
    {
        private readonly IEnumerable<IGuildNotifier> _guildNotifiers;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Rank;

        public GuildRankHandler(IPlayerInfoProvider playerInfoProvider,
                                IEnumerable<IGuildNotifier> guildNotifiers)
            : base(playerInfoProvider)
        {
            _guildNotifiers = guildNotifiers;
        }

        public override bool HandlePacket(GuildRankServerPacket packet)
        {
            foreach (var notifier in _guildNotifiers)
                notifier.NotifyRanks(packet.Ranks);

            return true;
        }
    }
}
