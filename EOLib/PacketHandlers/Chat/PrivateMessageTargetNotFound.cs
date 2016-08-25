// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public class PrivateMessageTargetNotFound : IPacketHandler
    {
        private const int TALK_NOTFOUND = 1;

        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringService _localizedStringService;
        private readonly ChatEventManager _chatEventManager;

        public PacketFamily Family { get { return PacketFamily.Talk; } }

        public PacketAction Action { get { return PacketAction.Reply; } }

        public bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        public PrivateMessageTargetNotFound(IPlayerInfoProvider playerInfoProvider,
                                            IChatRepository chatRepository,
                                            ILocalizedStringService localizedStringService,
                                            ChatEventManager chatEventManager)
        {
            _playerInfoProvider = playerInfoProvider;
            _chatRepository = chatRepository;
            _localizedStringService = localizedStringService;
            _chatEventManager = chatEventManager;
        }

        public bool HandlePacket(IPacket packet)
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

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }
    }
}
