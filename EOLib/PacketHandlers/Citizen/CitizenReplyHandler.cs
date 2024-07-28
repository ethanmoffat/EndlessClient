using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Citizen
{
    /// <summary>
    /// Sent when signing up to a town
    /// </summary>
    [AutoMappedType]
    public class CitizenReplyHandler : InGameOnlyPacketHandler<CitizenReplyServerPacket>
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

        public override bool HandlePacket(CitizenReplyServerPacket packet)
        {
            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyCitizenSignUp(packet.QuestionsWrong);

            return true;
        }
    }
}