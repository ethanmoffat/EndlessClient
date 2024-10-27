using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Extensions;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Optional;

namespace EOLib.Domain.Interact.Guild
{
    [AutoMappedType]
    public class GuildActions : IGuildActions
    {
        private readonly IGuildSessionRepository _guildSessionRepository;
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterRepository _characterRepository;

        public GuildActions(IGuildSessionRepository guildSessionRespository,
                          IPacketSendService packetSendService,
                          ICharacterRepository characterRepository)
        {
            _guildSessionRepository = guildSessionRespository;
            _packetSendService = packetSendService;
            _characterRepository = characterRepository;
        }

        public void Lookup(string identity)
        {
            _packetSendService.SendPacket(new GuildReportClientPacket { SessionId = _guildSessionRepository.SessionID, GuildIdentity = identity });
        }

        public void ViewMembers(string identity)
        {
            _packetSendService.SendPacket(new GuildTellClientPacket { SessionId = _guildSessionRepository.SessionID, GuildIdentity = identity });
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
            _packetSendService.SendPacket(new GuildAgreeClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                InfoType = GuildInfoType.Description,
                InfoTypeData = new GuildAgreeClientPacket.InfoTypeDataDescription
                {
                    Description = description
                }
            });
        }

        public void GetGuildBankBalance(string guildTag)
        {
            _packetSendService.SendPacket(new GuildTakeClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                InfoType = GuildInfoType.Bank,
                GuildTag = guildTag
            });
        }

        public void BankDeposit(int depositAmount)
        {
            _packetSendService.SendPacket(new GuildBuyClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                GoldAmount = depositAmount
            });
        }

        public void GetGuildRanks(string guildTag)
        {
            _packetSendService.SendPacket(new GuildTakeClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                InfoType = GuildInfoType.Ranks,
                GuildTag = guildTag
            });

        }

        public void SetGuildRanks(IReadOnlyList<string> ranks)
        {
            var ranksList = ranks.ToList();
            while (ranksList.Count != 9)
            {
                ranksList.Add(string.Empty);
            }

            _packetSendService.SendPacket(new GuildAgreeClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                InfoType = GuildInfoType.Ranks,
                InfoTypeData = new GuildAgreeClientPacket.InfoTypeDataRanks
                {
                    Ranks = ranksList
                }
            });
        }

        public void AssignRank(string playerName, int rank)
        {
            _guildSessionRepository.GuildPlayerModifyCandidate = playerName.Capitalize();

            _packetSendService.SendPacket(new GuildRankClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                MemberName = playerName,
                Rank = rank
            });
        }

        public void RequestToJoinGuild(string guildTag, string recruiterName)
        {
            _packetSendService.SendPacket(new GuildPlayerClientPacket { SessionId = _guildSessionRepository.SessionID, GuildTag = guildTag, RecruiterName = recruiterName });
        }

        public void LeaveGuild()
        {
            _packetSendService.SendPacket(new GuildRemoveClientPacket { SessionId = _guildSessionRepository.SessionID });
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithGuildTag("   ")
                .WithGuildName(string.Empty)
                .WithGuildRank(string.Empty);
        }

        public void RequestToCreateGuild(string guildTag, string guildName, string guildDescription)
        {
            _guildSessionRepository.CreationSession = Option.Some(
                new GuildCreationSession.Builder
                {
                    Tag = guildTag,
                    Name = guildName,
                    Description = guildDescription,
                    Members = new HashSet<string>()
                }.ToImmutable());

            _packetSendService.SendPacket(new GuildRequestClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                GuildTag = guildTag,
                GuildName = guildName,
            });
        }

        public void ConfirmGuildCreate(GuildCreationSession creationSession)
        {
            _packetSendService.SendPacket(new GuildCreateClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                GuildTag = creationSession.Tag,
                GuildName = creationSession.Name,
                Description = creationSession.Description,
            });
        }

        public void CancelGuildCreate()
        {
            _guildSessionRepository.CreationSession = Option.None<GuildCreationSession>();
        }

        public void KickMember(string member)
        {
            _guildSessionRepository.GuildPlayerModifyCandidate = member.Capitalize();

            _packetSendService.SendPacket(new GuildKickClientPacket
            {
                SessionId = _guildSessionRepository.SessionID,
                MemberName = member
            });
        }

        public void DisbandGuild()
        {
            _packetSendService.SendPacket(new GuildJunkClientPacket { SessionId = _guildSessionRepository.SessionID });
        }

        public void ClearLocalState()
        {
            _guildSessionRepository.GuildMembers.Clear();
            _guildSessionRepository.GuildInfo = Option.None<GuildInfo>();
        }
    }

    public interface IGuildActions
    {
        void Lookup(string identity);

        void ViewMembers(string identity);

        void GetGuildDescription(string guildTag);

        void SetGuildDescription(string description);

        void GetGuildBankBalance(string guildTag);

        void BankDeposit(int depositAmount);

        void GetGuildRanks(string guildTag);

        void SetGuildRanks(IReadOnlyList<string> ranks);

        void AssignRank(string playerName, int rank);

        void RequestToJoinGuild(string guildTag, string recruiterName);

        void LeaveGuild();

        void RequestToCreateGuild(string guildTag, string guildName, string guildDescription);

        void ConfirmGuildCreate(GuildCreationSession creationSession);

        void CancelGuildCreate();

        void KickMember(string responseText);

        void DisbandGuild();

        void ClearLocalState();
    }
}
