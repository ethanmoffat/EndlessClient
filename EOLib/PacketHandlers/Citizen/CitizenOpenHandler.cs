using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Citizen;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Citizen
{
    /// <summary>
    /// Sent when opening an Innkeeper dialog
    /// </summary>
    [AutoMappedType]
    public class CitizenOpenHandler : InGameOnlyPacketHandler
    {
        private readonly ICitizenDataRepository _citizenDataRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Citizen;

        public override PacketAction Action => PacketAction.Open;

        public CitizenOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICitizenDataRepository citizenDataRepository,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _citizenDataRepository = citizenDataRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            _citizenDataRepository.BehaviorID = Option.Some(packet.ReadThree());
            _citizenDataRepository.CurrentHomeID = packet.ReadChar().SomeWhen(x => x > 0);
            _currentMapStateRepository.MapWarpSession = Option.Some(packet.ReadShort());

            if (packet.ReadByte() != 255)
                return false;

            var question1 = packet.ReadBreakString();
            var question2 = packet.ReadBreakString();
            var question3 = packet.ReadEndString();

            _citizenDataRepository.Questions = new List<string> { question1, question2, question3 };

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Inn);

            return true;
        }
    }
}
