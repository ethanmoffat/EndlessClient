using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Citizen
{
    /// <summary>
    /// Sent when requesting to sleep at an inn
    /// </summary>
    [AutoMappedType]
    public class CitizenRequestHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Citizen;

        public override PacketAction Action => PacketAction.Request;

        public CitizenRequestHandler(IPlayerInfoProvider playerInfoProvider,
                                     IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var sleepCost = packet.ReadInt();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyCitizenRequestSleep(sleepCost);

            return true;
        }
    }
}
