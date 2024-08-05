using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Law;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Marriage
{
    [AutoMappedType]
    public class MarriageOpenHandler : InGameOnlyPacketHandler<MarriageOpenServerPacket>
    {
        private readonly ILawSessionRepository _lawSessionRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Marriage;

        public override PacketAction Action => PacketAction.Open;

        public MarriageOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                   ILawSessionRepository lawSessionRepository,
                                   IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _lawSessionRepository = lawSessionRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(MarriageOpenServerPacket packet)
        {
            _lawSessionRepository.SessionID = packet.SessionId;

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Law);

            return true;
        }
    }
}