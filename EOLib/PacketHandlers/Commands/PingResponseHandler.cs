using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Protocol;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Commands
{
    /// <summary>
    /// Handles MESSAGE_PONG packets which are in response to a #ping command request
    /// </summary>
    [AutoMappedType]
    public class PingResponseHandler : InGameOnlyPacketHandler<MessagePongServerPacket>
    {
        private readonly IPingTimeRepository _pingTimeRepository;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family => PacketFamily.Message;

        public override PacketAction Action => PacketAction.Pong;

        public PingResponseHandler(IPlayerInfoProvider playerInfoProvider,
                                   IPingTimeRepository pingTimeRepository,
                                   IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _pingTimeRepository = pingTimeRepository;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(MessagePongServerPacket packet)
        {
            var time = (int)_pingTimeRepository.RequestTimer.ElapsedMilliseconds;
            _pingTimeRepository.RequestTimer.Reset();

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyServerPing(time);

            return true;
        }
    }
}