using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Protocol;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Commands
{
    /// <summary>
    /// Handles MESSAGE_PONG packets which are in response to a #ping command request
    /// </summary>
    [AutoMappedType]
    public class PingResponseHandler : InGameOnlyPacketHandler
    {
        private readonly IPingTimeRepository _pingTimeRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family => PacketFamily.Message;

        public override PacketAction Action => PacketAction.Pong;

        public PingResponseHandler(IPingTimeRepository pingTimeRepository,
                                   IChatRepository chatRepository,
                                   IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _pingTimeRepository = pingTimeRepository;
            _chatRepository = chatRepository;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var now = DateTime.Now;
            short requestID = packet.ReadShort();
            if (!_pingTimeRepository.PingRequests.ContainsKey(requestID))
                return true;

            var timeInMS = (int) Math.Round((now - _pingTimeRepository.PingRequests[requestID]).TotalMilliseconds);
            _pingTimeRepository.PingRequests.Remove(requestID);

            var message = $"[x] Current ping to the server is: {timeInMS} ms.";
            var chatData = new ChatData(ChatTab.Local, "System", message, ChatIcon.LookingDude);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyChatReceived(ChatEventType.Server);

            return true;
        }
    }
}
