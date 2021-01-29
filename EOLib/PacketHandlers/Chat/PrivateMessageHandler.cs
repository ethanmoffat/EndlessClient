using System;
using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class PrivateMessageHandler : PlayerChatByNameBase
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action => PacketAction.Tell;

        public PrivateMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                     IChatRepository chatRepository)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        protected override void PostChat(string name, string message)
        {
            var localData = new ChatData(name, message, ChatIcon.Note, ChatColor.PM);
            var pmData = new ChatData(name, message, ChatIcon.Note);

            ChatTab whichPmTab;

            if (_chatRepository.PMTarget1 == null && _chatRepository.PMTarget2 == null)
                whichPmTab = ChatTab.Local;
            else
                whichPmTab = _chatRepository.PMTarget1.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    ? ChatTab.Private1
                    : _chatRepository.PMTarget2.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                        ? ChatTab.Private2
                        : ChatTab.Local;

            _chatRepository.AllChat[ChatTab.Local].Add(localData);
            if (whichPmTab != ChatTab.Local)
                _chatRepository.AllChat[whichPmTab].Add(pmData);
        }
    }
}