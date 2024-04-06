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

        public PingResponseHandler(IPlayerInfoProvider playerInfoProvider,
                                   IPingTimeRepository pingTimeRepository,
                                   IChatRepository chatRepository,
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
            var requestID = packet.ReadShort();
            if (!_pingTimeRepository.PingRequests.ContainsKey(requestID))
                return false;

            var timeInMS = (int) Math.Round((now - _pingTimeRepository.PingRequests[requestID]).TotalMilliseconds);
            _pingTimeRepository.PingRequests.Remove(requestID);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyServerPing(timeInMS);

            return true;
        }
    }
}
