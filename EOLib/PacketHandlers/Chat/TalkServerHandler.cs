using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class TalkServerHandler : InGameOnlyPacketHandler<TalkServerServerPacket>
    {
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family => PacketFamily.Talk;

        public override PacketAction Action => PacketAction.Server;

        public TalkServerHandler(IPlayerInfoProvider playerInfoProvider,
                                    IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(TalkServerServerPacket packet)
        {
            foreach (var notifier in _chatEventNotifiers)
            {
                notifier.NotifyServerMessage(packet.Message);
            }

            return true;
        }
    }
}
