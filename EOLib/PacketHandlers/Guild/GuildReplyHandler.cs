using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

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
                case GuildReply.JoinRequest:
                    {
                        var data = (GuildReplyServerPacket.ReplyCodeDataJoinRequest)(packet.ReplyCodeData);
                        foreach (var notifier in _guildNotifiers)
                            notifier.NotifyRequestToJoinGuild(data.PlayerId, data.Name);
                        break;
                    }
                case GuildReply.Updated:
                case GuildReply.NotFound:
                case GuildReply.RecruiterOffline:
                case GuildReply.RecruiterNotHere:
                case GuildReply.RecruiterWrongGuild:
                case GuildReply.NotRecruiter:
                case GuildReply.NotApproved:
                case GuildReply.Exists:
                case GuildReply.NoCandidates:
                case GuildReply.Busy:
                    {
                        foreach (var notifier in _guildNotifiers)
                            notifier.NotifyGuildReply(packet.ReplyCode);
                        break;
                    }
                case GuildReply.CreateBegin:
                    {
                        _guildSessionRepository.CreationSession.MatchSome(creationSession =>
                        {
                            creationSession.Approved = true;
                        });
                        break;
                    }
                case GuildReply.CreateAdd:
                    {
                        _guildSessionRepository.CreationSession.MatchSome(creationSession =>
                        {
                            var data = (GuildReplyServerPacket.ReplyCodeDataCreateAdd)(packet.ReplyCodeData);
                            if (!creationSession.Members.Contains(data.Name))
                                creationSession.Members.Add(data.Name);
                        });
                        
                        break;
                    }
                case GuildReply.CreateAddConfirm:
                    {
                        _guildSessionRepository.CreationSession.MatchSome(creationSession =>
                        {
                            var data = (GuildReplyServerPacket.ReplyCodeDataCreateAddConfirm)(packet.ReplyCodeData);
                            creationSession.Members.Add(data.Name);

                            foreach (var notifier in _guildNotifiers)
                                notifier.NotifyConfirmCreateGuild();
                        });
                        break;
                    }
            }

            return true;
        }
    }
}
