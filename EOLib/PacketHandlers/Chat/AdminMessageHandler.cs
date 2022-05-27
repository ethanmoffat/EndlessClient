using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class AdminMessageHandler : PlayerChatByNameBase
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

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(ChatTab.Group, name, message, ChatIcon.HGM, ChatColor.Admin);
            _chatRepository.AllChat[ChatTab.Group].Add(data);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.AdminChat);
        }
    }
}