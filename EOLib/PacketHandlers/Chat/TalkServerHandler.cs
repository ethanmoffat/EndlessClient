using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class TalkServerHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var serverMessage = packet.ReadEndString();

            foreach (var notifier in _chatEventNotifiers)
            {
                notifier.NotifyServerMessage(serverMessage);
            }

            return true;
        }
    }
}
