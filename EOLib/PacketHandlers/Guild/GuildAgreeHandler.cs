using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Extensions;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildAgreeHandler : InGameOnlyPacketHandler<GuildAgreeServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IGuildNotifier> _guildNotifiers;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Agree;

        public GuildAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 IEnumerable<IGuildNotifier> guildNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _guildNotifiers = guildNotifiers;
        }

        public override bool HandlePacket(GuildAgreeServerPacket packet)
        {
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithGuildTag(packet.GuildTag.ToUpper())
                .WithGuildName(packet.GuildName.Capitalize())
                .WithGuildRank(packet.RankName.Capitalize())
                .WithGuildRankID(9);

            foreach (var notifier in _guildNotifiers)
                notifier.NotifyAcceptedIntoGuild();

            return true;
        }
    }
}
