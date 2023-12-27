using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Priest;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Priest
{
    [AutoMappedType]
    public class PriestReplyHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Priest;

        public override PacketAction Action => PacketAction.Reply;

        public PriestReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                  IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var replyCode = (PriestReply)packet.ReadShort();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyPriestReply(replyCode);

            return true;
        }
    }
}
