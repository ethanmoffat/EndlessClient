using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.PacketHandlers.NPC;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Cast
{
    /// <summary>
    /// Sent when an NPC takes damage from a spell cast
    /// </summary>
    [AutoMappedType]
    public class CastReplyHandler : NPCTakeDamageHandler<CastReplyServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Cast;

        public CastReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository,
                                IEnumerable<INPCActionNotifier> npcNotifiers,
                                IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, npcNotifiers, otherCharacterAnimationNotifiers) { }

        public override bool HandlePacket(CastReplyServerPacket packet)
        {
            Handle(packet.CasterId, (EODirection)packet.CasterDirection,
                packet.NpcIndex, packet.Damage, packet.HpPercentage,
                packet.SpellId, packet.CasterTp);
            return true;
        }
    }
}
