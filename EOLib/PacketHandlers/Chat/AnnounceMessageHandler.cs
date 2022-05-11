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
        private readonly IEnumerable<IOtherCharacterEventNotifier> _notifiers;

        public override PacketAction Action => PacketAction.Announce;

        public AnnounceMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                      IChatRepository chatRepository,
                                      IEnumerable<IOtherCharacterEventNotifier> notifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _notifiers = notifiers;
        }

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(ChatTab.Global, name, message, ChatIcon.GlobalAnnounce, ChatColor.ServerGlobal);
            _chatRepository.AllChat[ChatTab.Local].Add(data);
            _chatRepository.AllChat[ChatTab.Global].Add(data);
            _chatRepository.AllChat[ChatTab.Group].Add(data);

            foreach (var notifier in _notifiers)
                notifier.AdminAnnounce(message);
        }
    }
}