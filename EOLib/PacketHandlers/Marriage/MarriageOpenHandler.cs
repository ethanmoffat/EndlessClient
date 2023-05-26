using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Law;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Marriage
{
    [AutoMappedType]
    public class MarriageOpenHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            _lawSessionRepository.SessionID = packet.ReadThree();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Law);

            return true;
        }
    }
}
