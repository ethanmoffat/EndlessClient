using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Priest;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Priest
{
    [AutoMappedType]
    public class PriestOpenHandler : InGameOnlyPacketHandler
    {
        private readonly IPriestSessionRepository _priestSessionRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Priest;

        public override PacketAction Action => PacketAction.Open;

        public PriestOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                 IPriestSessionRepository priestSessionRepository,
                                 IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _priestSessionRepository = priestSessionRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            _priestSessionRepository.SessionID = packet.ReadInt();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Priest);

            return true;
        }
    }
}
