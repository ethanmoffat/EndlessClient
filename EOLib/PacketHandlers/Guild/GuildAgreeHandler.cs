using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildAgreeHandler : InGameOnlyPacketHandler<GuildAgreeServerPacket>
    {
        private const byte JoinGuildSfx = 18;

        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IGuildNotifier> _guildNotifiers;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Agree;

        public GuildAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 IEnumerable<IGuildNotifier> guildNotifiers,
                                 IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _guildNotifiers = guildNotifiers;
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(GuildAgreeServerPacket packet)
        {
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithGuildTag(packet.GuildTag)
                .WithGuildName(packet.GuildName)
                .WithGuildRank(packet.RankName);

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(JoinGuildSfx);

            return true;
        }
    }
}
