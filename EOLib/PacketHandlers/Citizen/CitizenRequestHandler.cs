using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Citizen
{
    /// <summary>
    /// Sent when requesting to sleep at an inn
    /// </summary>
    [AutoMappedType]
    public class CitizenRequestHandler : InGameOnlyPacketHandler<CitizenRequestServerPacket>
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

        public override bool HandlePacket(CitizenRequestServerPacket packet)
        {
            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyCitizenRequestSleep(packet.Cost);

            return true;
        }
    }
}