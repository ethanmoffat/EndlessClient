using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class AdminMessageHandler : PlayerChatByNameBase<TalkAdminServerPacket>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketAction Action => PacketAction.Admin;

        public AdminMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                   IChatRepository chatRepository,
                                   IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(TalkAdminServerPacket packet)
        {
            return Handle(packet.PlayerName, packet.Message);
        }

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(ChatTab.Group, name, message, ChatIcon.HGM, ChatColor.Admin);
            _chatRepository.AllChat[ChatTab.Group].Add(data);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.AdminChat);
        }
    }
}