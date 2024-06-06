using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class GroupChatHandler : InGameOnlyPacketHandler<TalkOpenServerPacket>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IPartyDataProvider _partyDataProvider;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family => PacketFamily.Talk;
        public override PacketAction Action => PacketAction.Open;

        public GroupChatHandler(IPlayerInfoProvider playerInfoProvider,
                                IChatRepository chatRepository,
                                IPartyDataProvider partyDataProvider,
                                IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers,
                                IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _partyDataProvider = partyDataProvider;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(TalkOpenServerPacket packet)
        {
            var member = _partyDataProvider.Members.FirstOrDefault(member => member.CharacterID == packet.PlayerId);
            if (member == null)
                return true;

            var localChatData = new ChatData(ChatTab.Local, member.Name, packet.Message, ChatIcon.PlayerPartyDark, ChatColor.PM);
            _chatRepository.AllChat[ChatTab.Local].Add(localChatData);

            var chatData = new ChatData(ChatTab.Group, member.Name, packet.Message, ChatIcon.PlayerPartyDark);
            _chatRepository.AllChat[ChatTab.Group].Add(chatData);

            foreach (var notifier in _otherCharacterEventNotifiers)
                notifier.OtherCharacterSaySomethingToGroup(member.CharacterID, packet.Message);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.Group);

            return true;
        }
    }
}
