using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;
using EOLib.Domain.Interact.Barber;
using EOLib.Domain.Character;
using EOLib.Domain.Login;

namespace EOLib.PacketHandlers.Barber
{
    [AutoMappedType]
    public class BarberOpenHandler : InGameOnlyPacketHandler
    {
        private readonly IBarberDataRepository _barberDataRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Barber;
        public override PacketAction Action => PacketAction.Open;

        public BarberOpenHandler(
            IPlayerInfoProvider playerInfoProvider,
            IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers,
            IBarberDataRepository barberDataRepository)
            : base(playerInfoProvider)
        {
            _npcInteractionNotifiers = npcInteractionNotifiers;
            _barberDataRepository = barberDataRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var sessionId = packet.ReadInt();
            _barberDataRepository.SessionID = sessionId;

            foreach (var notifier in _npcInteractionNotifiers)
            {
                notifier.NotifyInteractionFromNPC(IO.NPCType.Barber);
            }

            return true;
        }
    }
}
