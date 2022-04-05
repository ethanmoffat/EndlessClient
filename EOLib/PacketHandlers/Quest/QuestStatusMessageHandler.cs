using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Quest
{
    [AutoMappedType]
    public class QuestStatusMessageHandler : InGameOnlyPacketHandler
    {
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<IStatusLabelNotifier> _statusLabelNotifiers;

        public override PacketFamily Family => PacketFamily.Message;

        public override PacketAction Action => PacketAction.Open;

        public QuestStatusMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                         IChatRepository chatRepository,
                                         IEnumerable<IStatusLabelNotifier> statusLabelNotifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _statusLabelNotifiers = statusLabelNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var message = packet.ReadEndString();

            foreach (var notifier in _statusLabelNotifiers)
                notifier.ShowWarning(message);

            var chatData = new ChatData(string.Empty, message, ChatIcon.QuestMessage, ChatColor.Server);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);

            return true;
        }
    }
}
