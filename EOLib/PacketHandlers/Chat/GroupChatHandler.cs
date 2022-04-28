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
        private readonly IEnumerable<IOtherCharacterEventNotifier> _notifiers;

        public override PacketAction Action => PacketAction.Open;

        public GroupChatHandler(ICurrentMapStateProvider currentMapStateProvider,
                                IPlayerInfoProvider playerInfoProvider,
                                IChatRepository chatRepository,
                                IEnumerable<IOtherCharacterEventNotifier> notifiers)
            : base(currentMapStateProvider, playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _notifiers = notifiers;
        }

        protected override void DoTalk(IPacket packet, Character character)
        {
            var message = packet.ReadBreakString();

            var chatData = new ChatData(character.Name, message, ChatIcon.PlayerPartyDark);
            _chatRepository.AllChat[ChatTab.Group].Add(chatData);

            foreach (var notifier in _notifiers)
                notifier.OtherCharacterSaySomethingToGroup(character.ID, message);
        }
    }
}