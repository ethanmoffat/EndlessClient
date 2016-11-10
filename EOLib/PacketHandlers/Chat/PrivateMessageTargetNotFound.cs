// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public class PrivateMessageTargetNotFound : InGameOnlyPacketHandler
    {
        private const int TALK_NOTFOUND = 1;

        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringService _localizedStringService;
        private readonly ChatEventManager _chatEventManager;

        public override PacketFamily Family { get { return PacketFamily.Talk; } }

        public override PacketAction Action { get { return PacketAction.Reply; } }

        public PrivateMessageTargetNotFound(IPlayerInfoProvider playerInfoProvider,
                                            IChatRepository chatRepository,
                                            ILocalizedStringService localizedStringService,
                                            ChatEventManager chatEventManager)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _localizedStringService = localizedStringService;
            _chatEventManager = chatEventManager;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var response = packet.ReadShort();
            if (response != TALK_NOTFOUND)
                return false;

            var from = packet.ReadEndString();
            from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
            var sysMessage = _localizedStringService.GetString(EOResourceID.SYS_CHAT_PM_PLAYER_COULD_NOT_BE_FOUND);
            var message = string.Format("{0} {1}", from, sysMessage);

            var chatData = new ChatData(string.Empty, message, ChatIcon.Error, ChatColor.Error);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);
            _chatEventManager.FireChatPMTargetNotFound(from);

            return true;
        }
    }
}
