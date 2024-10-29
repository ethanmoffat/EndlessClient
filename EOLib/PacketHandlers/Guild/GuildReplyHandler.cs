using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]
    public class GuildReplyHandler : InGameOnlyPacketHandler<GuildReplyServerPacket>
    {
        private readonly IEnumerable<IGuildNotifier> _guildNotifiers;
        private readonly IGuildSessionRepository _guildSessionRepository;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Reply;

        public GuildReplyHandler(IPlayerInfoProvider playerInfoProvider,
            IEnumerable<IGuildNotifier> guildNotifiers,
            IGuildSessionRepository guildSessionRepository)
            : base(playerInfoProvider)
        {
            _guildNotifiers = guildNotifiers;
            _guildSessionRepository = guildSessionRepository;
        }

        public override bool HandlePacket(GuildReplyServerPacket packet)
        {
            switch (packet.ReplyCode)
            {
                case GuildReply.Busy:
                case GuildReply.NotApproved:
                case GuildReply.AlreadyMember:
                case GuildReply.NoCandidates:
                case GuildReply.Exists:
                case GuildReply.RecruiterOffline:
                case GuildReply.RecruiterNotHere:
                case GuildReply.RecruiterWrongGuild:
                case GuildReply.NotRecruiter:
                case GuildReply.NotPresent:
                case GuildReply.AccountLow:
                case GuildReply.Accepted:
                case GuildReply.NotFound:
                case GuildReply.Updated:
                case GuildReply.RanksUpdated:
                case GuildReply.RemoveLeader:
                case GuildReply.RemoveNotMember:
                case GuildReply.Removed:
                case GuildReply.RankingLeader:
                case GuildReply.RankingNotMember:
                    {
                        foreach (var notifier in _guildNotifiers)
                            notifier.NotifyGuildReply(packet.ReplyCode);
                        break;
                    }
                case GuildReply.CreateBegin:
                    {
                        _guildSessionRepository.CreationSession.MatchSome(creationSession =>
                        {
                            _guildSessionRepository.CreationSession = Option.Some(creationSession.WithApproved(true));
                        });
                        break;
                    }
                case GuildReply.CreateAddConfirm:
                    {
                        _guildSessionRepository.CreationSession.MatchSome(creationSession =>
                        {
                            var data = (GuildReplyServerPacket.ReplyCodeDataCreateAddConfirm)packet.ReplyCodeData;
                            var updatedMemberList = new HashSet<string>(creationSession.Members) { data.Name };
                            _guildSessionRepository.CreationSession = Option.Some(creationSession.WithMembers(updatedMemberList));

                            foreach (var notifier in _guildNotifiers)
                                notifier.NotifyConfirmCreateGuild();
                        });
                        break;
                    }
                case GuildReply.CreateAdd:
                    {
                        _guildSessionRepository.CreationSession.MatchSome(creationSession =>
                        {
                            var data = (GuildReplyServerPacket.ReplyCodeDataCreateAdd)packet.ReplyCodeData;
                            var updatedMemberList = new HashSet<string>(creationSession.Members) { data.Name };
                            _guildSessionRepository.CreationSession = Option.Some(creationSession.WithMembers(updatedMemberList));
                        });
                        
                        break;
                    }
                case GuildReply.JoinRequest:
                    {
                        var data = (GuildReplyServerPacket.ReplyCodeDataJoinRequest)(packet.ReplyCodeData);
                        foreach (var notifier in _guildNotifiers)
                            notifier.NotifyRequestToJoinGuild(data.PlayerId, data.Name);
                        break;
                    }

            }

            return true;
        }
    }
}
