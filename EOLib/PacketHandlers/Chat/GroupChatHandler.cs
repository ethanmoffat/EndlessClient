using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class GroupChatHandler : PlayerChatByIDHandler<TalkOpenServerPacket>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketAction Action => PacketAction.Open;

        public GroupChatHandler(IPlayerInfoProvider playerInfoProvider,
                                ICurrentMapStateProvider currentMapStateProvider,
                                ICharacterProvider characterProvider,
                                IChatRepository chatRepository,
                                IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers,
                                IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider, currentMapStateProvider, characterProvider)
        {
            _chatRepository = chatRepository;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(TalkOpenServerPacket packet)
        {
            return Handle(packet, packet.PlayerId);
        }

        protected override void DoTalk(TalkOpenServerPacket packet, Character character)
        {
            var localChatData = new ChatData(ChatTab.Local, character.Name, packet.Message, ChatIcon.PlayerPartyDark, ChatColor.PM);
            _chatRepository.AllChat[ChatTab.Local].Add(localChatData);

            var chatData = new ChatData(ChatTab.Group, character.Name, packet.Message, ChatIcon.PlayerPartyDark);
            _chatRepository.AllChat[ChatTab.Group].Add(chatData);

            foreach (var notifier in _otherCharacterEventNotifiers)
                notifier.OtherCharacterSaySomethingToGroup(character.ID, packet.Message);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.Group);
        }
    }
}
