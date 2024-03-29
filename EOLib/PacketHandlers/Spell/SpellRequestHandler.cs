﻿using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Spell
{
    /// <summary>
    /// Sent when a player starts a spell chant
    /// </summary>
    [AutoMappedType]
    public class SpellRequestHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> animationNotifiers;

        public override PacketFamily Family => PacketFamily.Spell;
        public override PacketAction Action => PacketAction.Request;

        public SpellRequestHandler(IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<IOtherCharacterAnimationNotifier> animationNotifiers)
            : base(playerInfoProvider)
        {
            this.animationNotifiers = animationNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerId = packet.ReadShort();
            var spellId = packet.ReadShort();

            foreach (var notifier in animationNotifiers)
                notifier.NotifyStartSpellCast(playerId, spellId);

            return true;
        }
    }
}
