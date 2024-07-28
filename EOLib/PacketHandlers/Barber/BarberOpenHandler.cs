using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Barber;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Barber
{
    [AutoMappedType]
    public class BarberOpenHandler : InGameOnlyPacketHandler<BarberOpenServerPacket>
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

        public override bool HandlePacket(BarberOpenServerPacket packet)
        {
            _barberDataRepository.SessionID = packet.SessionId;

            foreach (var notifier in _npcInteractionNotifiers)
            {
                notifier.NotifyInteractionFromNPC(IO.NPCType.Barber);
            }

            return true;
        }
    }
}