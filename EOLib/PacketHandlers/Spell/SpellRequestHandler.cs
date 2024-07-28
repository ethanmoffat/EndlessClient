using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Spell
{
    /// <summary>
    /// Sent when a player starts a spell chant
    /// </summary>
    [AutoMappedType]
    public class SpellRequestHandler : InGameOnlyPacketHandler<SpellRequestServerPacket>
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

        public override bool HandlePacket(SpellRequestServerPacket packet)
        {
            foreach (var notifier in animationNotifiers)
                notifier.NotifyStartSpellCast(packet.PlayerId, packet.SpellId);

            return true;
        }
    }
}