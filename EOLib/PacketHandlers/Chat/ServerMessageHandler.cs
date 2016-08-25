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
    public class ServerMessageHandler : IPacketHandler
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringService _localizedStringService;

        public PacketFamily Family { get { return PacketFamily.Talk; } }

        public PacketAction Action { get { return PacketAction.Server; } }

        public bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        public ServerMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                    IChatRepository chatRepository,
                                    ILocalizedStringService localizedStringService)
        {
            _playerInfoProvider = playerInfoProvider;
            _chatRepository = chatRepository;
            _localizedStringService = localizedStringService;
        }

        public bool HandlePacket(IPacket packet)
        {
            var server = _localizedStringService.GetString(EOResourceID.STRING_SERVER);
            var serverMessage = packet.ReadEndString();

            var localData = new ChatData(server, serverMessage, ChatIcon.Exclamation, ChatColor.Server);
            var globalData = new ChatData(server, serverMessage, ChatIcon.Exclamation, ChatColor.ServerGlobal);
            var systemData = new ChatData("", serverMessage, ChatIcon.Exclamation, ChatColor.Server);

            _chatRepository.AllChat[ChatTab.Local].Add(localData);
            _chatRepository.AllChat[ChatTab.Global].Add(globalData);
            _chatRepository.AllChat[ChatTab.System].Add(systemData);

            return true;
        }

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }
    }
}
