using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class GroupChatHandler : PlayerChatByIDHandler
    {
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketAction Action => PacketAction.Open;

        public GroupChatHandler(ICurrentMapStateProvider currentMapStateProvider,
                                IPlayerInfoProvider playerInfoProvider,
                                IChatRepository chatRepository,
                                IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers,
                                IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(currentMapStateProvider, playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
            _chatEventNotifiers = chatEventNotifiers;
        }

        protected override void DoTalk(IPacket packet, Character character)
        {
            var message = packet.ReadBreakString();

            var chatData = new ChatData(ChatTab.Group, character.Name, message, ChatIcon.PlayerPartyDark);
            _chatRepository.AllChat[ChatTab.Group].Add(chatData);

            foreach (var notifier in _otherCharacterEventNotifiers)
                notifier.OtherCharacterSaySomethingToGroup(character.ID, message);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.Group);
        }
    }
}