using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Notifiers;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Interact.Guild
{
    [AutoMappedType]
    public class GuildActions : IGuildActions
    {
        private const byte LeaveGuildSfx = 6;

        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public GuildActions(IGuildSessionProvider guildSessionProvider,
                          IPacketSendService packetSendService,
                          ICharacterRepository characterRepository,
                          IEnumerable<ISoundNotifier> soundNotifiers)
        {
            _guildSessionProvider = guildSessionProvider;
            _packetSendService = packetSendService;
            _characterRepository = characterRepository;
            _soundNotifiers = soundNotifiers;
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

        public void LeaveGuild()
        {
            _packetSendService.SendPacket(new GuildRemoveClientPacket { SessionId = _guildSessionProvider.SessionID });

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithGuildTag("   ").WithGuildName("").WithGuildRank("");

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(LeaveGuildSfx);
        }
    }

    public interface IGuildActions
    {
        void Lookup(string identity);
        void ViewMembers(string identity);
        void RequestToJoinGuild(string guildTag, string recruiterName);
        void LeaveGuild();
    }
}
