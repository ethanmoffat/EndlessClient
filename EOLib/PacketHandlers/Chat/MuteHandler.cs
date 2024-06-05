using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class MuteHandler : InGameOnlyPacketHandler<TalkSpecServerPacket>
    {
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family => PacketFamily.Talk;

        public override PacketAction Action => PacketAction.Spec;

        public MuteHandler(IPlayerInfoProvider playerInfoProvider,
                           IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(TalkSpecServerPacket packet)
        {
            var adminName = packet.AdminName;
            adminName = char.ToUpper(adminName[0]) + adminName.Substring(1).ToLower();

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyPlayerMutedByAdmin(adminName);

            return true;
        }
    }
}
