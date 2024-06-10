using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System.Collections.Generic;
using System.Diagnostics;

namespace EOLib.PacketHandlers.Guild
{
    [AutoMappedType]

    public class GuildOpenHandler : InGameOnlyPacketHandler<GuildOpenServerPacket>
    {
        private readonly IGuildSessionRepository _guildSessionRepository;

        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Guild;

        public override PacketAction Action => PacketAction.Open;

        public GuildOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                 IGuildSessionRepository guildSessionRepository,
                                 IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _guildSessionRepository = guildSessionRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(GuildOpenServerPacket packet)
        {
            _guildSessionRepository.SessionID = packet.SessionId;
          
            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Guild);

            return true;
        }
    }
}
