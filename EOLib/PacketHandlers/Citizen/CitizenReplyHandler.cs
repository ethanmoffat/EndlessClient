using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Citizen
{
    /// <summary>
    /// Sent when signing up to a town
    /// </summary>
    [AutoMappedType]
    public class CitizenReplyHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Citizen;

        public override PacketAction Action => PacketAction.Reply;

        public CitizenReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var questionsWrong = packet.ReadChar();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyCitizenSignUp(questionsWrong);

            return true;
        }
    }
}
