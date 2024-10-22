using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Notifiers;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Optional;

namespace EOLib.Domain.Interact.Guild
{
    [AutoMappedType]
    public class GuildActions : IGuildActions
    {
        private const byte LeaveGuildSfx = 6;

        private readonly IGuildSessionRepository _guildSessionRepository;
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;

        public GuildActions(IGuildSessionRepository guildSessionRespository,
                          IPacketSendService packetSendService,
                          ICharacterRepository characterRepository,
                          IEnumerable<ISoundNotifier> soundNotifiers)
        {
            _guildSessionRepository = guildSessionRespository;
            _packetSendService = packetSendService;
            _characterRepository = characterRepository;
            _soundNotifiers = soundNotifiers;
        }

        public void Lookup(string identity)
        {
            _packetSendService.SendPacket(new GuildReportClientPacket { SessionId = _guildSessionRepository.SessionID, GuildIdentity = identity });
        }

        public void ViewMembers(string identity)
        {
            _packetSendService.SendPacket(new GuildTellClientPacket { SessionId = _guildSessionRepository.SessionID, GuildIdentity = identity });
        }

        public void RequestToJoinGuild(string guildTag, string recruiterName)
        {
            _packetSendService.SendPacket(new GuildPlayerClientPacket { SessionId = _guildSessionRepository.SessionID, GuildTag = guildTag, RecruiterName = recruiterName });
        }

        public void LeaveGuild()
        {
            _packetSendService.SendPacket(new GuildRemoveClientPacket { SessionId = _guildSessionRepository.SessionID });

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithGuildTag("   ").WithGuildName("").WithGuildRank("");

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(LeaveGuildSfx);
        }

        public void RequestToCreateGuild(string guildTag, string guildName, string guildDescription)
        {
            _guildSessionRepository.CreationSession = Option.Some(
                new GuildCreationSession.Builder
                {
                    Tag = guildTag,
                    Name = guildName,
                    Description = guildDescription
                }.ToImmutable());

            _packetSendService.SendPacket(new GuildRequestClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                GuildTag = guildTag,
                GuildName = guildName,
            });
        }

        public void GetGuildDescription(string guildTag)
        {
            _packetSendService.SendPacket(new GuildTakeClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                InfoType = GuildInfoType.Description,
                GuildTag = guildTag
            });
        }

        public void SetGuildDescription(string description)
        {
            _packetSendService.SendPacket(new GuildAgreeClientPacket()
            {
                SessionId = _guildSessionRepository.SessionID,
                InfoType = GuildInfoType.Description,
                InfoTypeData = new GuildAgreeClientPacket.InfoTypeDataDescription()
                {
                    Description = description
                }
            });
        }
    }

    public interface IGuildActions
    {
        void Lookup(string identity);

        void ViewMembers(string identity);

        void GetGuildDescription(string guildTag);

        void SetGuildDescription(string description);

        void RequestToJoinGuild(string guildTag, string recruiterName);

        void LeaveGuild();

        void RequestToCreateGuild(string guildTag, string guildName, string guildDescription);
    }
}
