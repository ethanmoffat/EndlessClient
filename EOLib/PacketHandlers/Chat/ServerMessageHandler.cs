using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Chat
{
    [AutoMappedType]
    public class ServerMessageHandler : InGameOnlyPacketHandler
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family => PacketFamily.Talk;

        public override PacketAction Action => PacketAction.Server;

        public ServerMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                    IChatRepository chatRepository,
                                    ILocalizedStringFinder localizedStringFinder,
                                    IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var server = _localizedStringFinder.GetString(EOResourceID.STRING_SERVER);
            var serverMessage = packet.ReadEndString();

            var localData = new ChatData(ChatTab.Local, server, serverMessage, ChatIcon.Exclamation, ChatColor.Server, log: false);
            var globalData = new ChatData(ChatTab.Global, server, serverMessage, ChatIcon.Exclamation, ChatColor.ServerGlobal, log: false);
            var systemData = new ChatData(ChatTab.System, string.Empty, serverMessage, ChatIcon.Exclamation, ChatColor.Server);

            _chatRepository.AllChat[ChatTab.Local].Add(localData);
            _chatRepository.AllChat[ChatTab.Global].Add(globalData);
            _chatRepository.AllChat[ChatTab.System].Add(systemData);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.Server);

            return true;
        }
    }
}
