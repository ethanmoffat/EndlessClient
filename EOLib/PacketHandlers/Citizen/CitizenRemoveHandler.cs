using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Citizen;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Citizen
{
    /// <summary>
    /// Sent when unsubscribing from a town
    /// </summary>
    [AutoMappedType]
    public class CitizenRemoveHandler : InGameOnlyPacketHandler<CitizenRemoveServerPacket>
    {
        private readonly ICitizenDataRepository _citizenDataRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Citizen;

        public override PacketAction Action => PacketAction.Remove;

        public CitizenRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICitizenDataRepository citizenDataRepository,
                                    IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _citizenDataRepository = citizenDataRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(CitizenRemoveServerPacket packet)
        {
            if (packet.ReplyCode == InnUnsubscribeReply.Unsubscribed)
                _citizenDataRepository.CurrentHomeID = Option.None<int>();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyCitizenUnsubscribe(packet.ReplyCode);

            return true;
        }
    }
}
