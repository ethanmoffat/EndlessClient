using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class PublicChatHandler : PlayerChatByIDHandler<TalkPlayerServerPacket>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _notifiers;

        public override PacketAction Action => PacketAction.Player;

        public PublicChatHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 ICharacterProvider characterProvider,
                                 IChatRepository chatRepository,
                                 IEnumerable<IOtherCharacterEventNotifier> notifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterProvider)
        {
            _chatRepository = chatRepository;
            _notifiers = notifiers;
        }

        public override bool HandlePacket(TalkPlayerServerPacket packet)
        {
            return Handle(packet, packet.PlayerId);
        }

        protected override void DoTalk(TalkPlayerServerPacket packet, Character character)
        {
            var chatData = new ChatData(ChatTab.Local, character.Name, packet.Message, ChatIcon.SpeechBubble);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            foreach (var notifier in _notifiers)
                notifier.OtherCharacterSaySomething(character.ID, packet.Message);
        }
    }
}
