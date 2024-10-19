﻿using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Priest;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Priest
{
    [AutoMappedType]
    public class PriestRequestHandler : InGameOnlyPacketHandler<PriestRequestServerPacket>
    {
        private readonly IPriestSessionRepository _priestSessionRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.Priest;

        public override PacketAction Action => PacketAction.Request;

        public PriestRequestHandler(IPlayerInfoProvider playerInfoProvider,
                                    IPriestSessionRepository priestSessionRepository,
                                    IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _priestSessionRepository = priestSessionRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(PriestRequestServerPacket packet)
        {
            _priestSessionRepository.SessionID = packet.SessionId;

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyPriestRequest(packet.PartnerName);

            return true;
        }
    }
}
