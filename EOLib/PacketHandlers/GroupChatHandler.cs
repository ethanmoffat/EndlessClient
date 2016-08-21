// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;

namespace EOLib.PacketHandlers
{
    public class GroupChatHandler : PlayerChatByIDHandler
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action { get { return PacketAction.Open; } }

        public GroupChatHandler(ICurrentMapStateProvider currentMapStateProvider,
                                IPlayerInfoProvider playerInfoProvider,
                                IChatRepository chatRepository)
            : base(currentMapStateProvider, playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        protected override void DoTalk(IPacket packet, ICharacter character)
        {
            var message = packet.ReadBreakString();

            var chatData = new ChatData(character.Name, message, ChatIcon.PlayerPartyDark);
            _chatRepository.AllChat[ChatTab.Group].Add(chatData);
        }
    }
}