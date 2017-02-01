// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
    public class PublicChatHandler : PlayerChatByIDHandler
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action => PacketAction.Player;

        public PublicChatHandler(ICurrentMapStateProvider currentMapStateProvider,
                                 IPlayerInfoProvider playerInfoProvider,
                                 IChatRepository chatRepository)
            : base(currentMapStateProvider, playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        protected override void DoTalk(IPacket packet, ICharacter character)
        {
            //todo: speech bubble!
            var message = packet.ReadEndString();

            var chatData = new ChatData(character.Name, message, ChatIcon.SpeechBubble);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);
        }
    }
}