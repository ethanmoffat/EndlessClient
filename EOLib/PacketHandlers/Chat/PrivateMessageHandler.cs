using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class PrivateMessageHandler : PlayerChatByNameBase<TalkTellServerPacket>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketAction Action => PacketAction.Tell;

        public PrivateMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                     IChatRepository chatRepository,
                                     IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(TalkTellServerPacket packet)
        {
            return Handle(packet.PlayerName, packet.Message);
        }

        protected override void PostChat(string name, string message)
        {
            var localData = new ChatData(ChatTab.Local, name, message, ChatIcon.Note, ChatColor.PM, log: false);

            ChatTab whichPmTab;
            if (_chatRepository.PMTarget1 == null && _chatRepository.PMTarget2 == null)
                whichPmTab = ChatTab.Local;
            else
                whichPmTab = _chatRepository.PMTarget1.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    ? ChatTab.Private1
                    : _chatRepository.PMTarget2.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                        ? ChatTab.Private2
                        : ChatTab.Local;

            var pmData = new ChatData(whichPmTab, name, message, ChatIcon.Note);

            _chatRepository.AllChat[ChatTab.Local].Add(localData);
            if (whichPmTab != ChatTab.Local)
                _chatRepository.AllChat[whichPmTab].Add(pmData);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.PrivateMessage);
        }
    }
}
