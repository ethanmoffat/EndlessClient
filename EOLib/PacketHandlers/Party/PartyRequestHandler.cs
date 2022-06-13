using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles invite/join request from another player
    /// </summary>
    [AutoMappedType]
    public class PartyRequestHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IPartyEventNotifier> _partyEventNotifiers;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Request;

        public PartyRequestHandler(IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<IPartyEventNotifier> partyEventNotifiers)
            : base(playerInfoProvider)
        {
            _partyEventNotifiers = partyEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var type = (PartyRequestType)packet.ReadChar();
            var playerId = packet.ReadShort();
            var name = packet.ReadEndString();

            foreach (var notifier in _partyEventNotifiers)
                notifier.NotifyPartyRequest(type, playerId, name);

            return true;
        }
    }
}
