using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Priest
{
    [AutoMappedType]
    public class PriestReplyHandler : InGameOnlyPacketHandler<PriestReplyServerPacket>
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

        public override bool HandlePacket(PriestReplyServerPacket packet)
        {
            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyPriestReply(packet.ReplyCode);

            return true;
        }
    }
}
