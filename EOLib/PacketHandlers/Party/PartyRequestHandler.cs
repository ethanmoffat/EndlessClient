using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles invite/join request from another player
    /// </summary>
    [AutoMappedType]
    public class PartyRequestHandler : InGameOnlyPacketHandler<PartyRequestServerPacket>
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

        public override bool HandlePacket(PartyRequestServerPacket packet)
        {
            foreach (var notifier in _partyEventNotifiers)
                notifier.NotifyPartyRequest(packet.RequestType, packet.InviterPlayerId, packet.PlayerName);

            return true;
        }
    }
}