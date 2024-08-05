using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Localization;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class PrivateMessageTargetNotFound : InGameOnlyPacketHandler<TalkReplyServerPacket>
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family => PacketFamily.Talk;

        public override PacketAction Action => PacketAction.Reply;

        public PrivateMessageTargetNotFound(IPlayerInfoProvider playerInfoProvider,
                                            IChatRepository chatRepository,
                                            ILocalizedStringFinder localizedStringFinder,
                                            IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(TalkReplyServerPacket packet)
        {
            if (packet.ReplyCode != TalkReply.NotFound)
                return true;

            var from = packet.Name;
            from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
            var sysMessage = _localizedStringFinder.GetString(EOResourceID.SYS_CHAT_PM_PLAYER_COULD_NOT_BE_FOUND);
            var message = $"{@from} {sysMessage}";

            var chatData = new ChatData(ChatTab.System, string.Empty, message, ChatIcon.Error, ChatColor.Error);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyPrivateMessageRecipientNotFound(from);

            return true;
        }
    }
}