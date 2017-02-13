// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
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

        protected override void DoTalk(IPacket packet, ICharacter character)
        {
            var message = packet.ReadBreakString();

            var chatData = new ChatData(character.Name, message, ChatIcon.PlayerPartyDark);
            _chatRepository.AllChat[ChatTab.Group].Add(chatData);

            foreach (var notifier in _notifiers)
                notifier.OtherCharacterSaySomething(character.ID, message);
        }
    }
}