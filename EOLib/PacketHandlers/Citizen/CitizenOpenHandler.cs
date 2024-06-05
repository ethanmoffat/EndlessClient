using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Citizen;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Citizen
{
    /// <summary>
    /// Sent when opening an Innkeeper dialog
    /// </summary>
    [AutoMappedType]
    public class CitizenOpenHandler : InGameOnlyPacketHandler<CitizenOpenServerPacket>
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

        public override bool HandlePacket(CitizenOpenServerPacket packet)
        {
            _citizenDataRepository.BehaviorID = Option.Some(packet.BehaviorId);
            _citizenDataRepository.CurrentHomeID = packet.CurrentHomeId.SomeWhen(x => x > 0);
            _currentMapStateRepository.MapWarpSession = Option.Some(packet.SessionId);

            _citizenDataRepository.Questions = packet.Questions;

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Inn);

            return true;
        }
    }
}
