using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class AnnounceMessageHandler : PlayerChatByNameBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketAction Action => PacketAction.Announce;

        public AnnounceMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                      IChatRepository chatRepository,
                                      IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers,
                                      IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
            _chatEventNotifiers = chatEventNotifiers;
        }

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(ChatTab.Global, name, message, ChatIcon.GlobalAnnounce, ChatColor.ServerGlobal);
            _chatRepository.AllChat[ChatTab.Local].Add(data);
            _chatRepository.AllChat[ChatTab.Global].Add(data);
            _chatRepository.AllChat[ChatTab.Group].Add(data);

            foreach (var notifier in _otherCharacterEventNotifiers)
                notifier.AdminAnnounce(message);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.AdminAnnounce);
        }
    }
}