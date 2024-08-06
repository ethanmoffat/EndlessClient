﻿using System.Collections.Generic;
using AutomaticTypeMapper;
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

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Reply;

        public GuildReplyHandler(IPlayerInfoProvider playerInfoProvider,
            IEnumerable<IGuildNotifier> guildNotifiers)
            : base(playerInfoProvider)
        {
            _guildNotifiers = guildNotifiers;
        }

        public override bool HandlePacket(GuildReplyServerPacket packet)
        {
            switch (packet.ReplyCode)
            {
                case GuildReply.JoinRequest:
                    var data = (GuildReplyServerPacket.ReplyCodeDataJoinRequest)(packet.ReplyCodeData);
                    foreach (var notifier in _guildNotifiers)
                        notifier.NotifyRequestToJoinGuild(data.PlayerId, data.Name);
                    break;
            }

            return true;
        }
    }
}
