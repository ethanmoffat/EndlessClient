// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Commands
{
    /// <summary>
    /// Handles MESSAGE_PONG packets which are in response to a #ping command request
    /// </summary>
    public class PingResponseHandler : IPacketHandler
    {
        private readonly IPingTimeRepository _pingTimeRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public PacketFamily Family { get { return PacketFamily.Message; } }

        public PacketAction Action { get { return PacketAction.Pong; } }

        public bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        public PingResponseHandler(IPingTimeRepository pingTimeRepository,
                                   IChatRepository chatRepository,
                                   IPlayerInfoProvider playerInfoProvider)
        {
            _pingTimeRepository = pingTimeRepository;
            _chatRepository = chatRepository;
            _playerInfoProvider = playerInfoProvider;
        }

        public bool HandlePacket(IPacket packet)
        {
            var now = DateTime.Now;
            ushort requestID = (ushort)packet.ReadShort();
            if (!_pingTimeRepository.PingRequests.ContainsKey(requestID))
                return true;

            var timeInMS = (int) Math.Round((now - _pingTimeRepository.PingRequests[requestID]).TotalMilliseconds);
            _pingTimeRepository.PingRequests.Remove(requestID);

            var message = string.Format("[x] Current ping to the server is: {0} ms.", timeInMS);
            var chatData = new ChatData("System", message, ChatIcon.LookingDude);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            return true;
        }

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }
    }
}
