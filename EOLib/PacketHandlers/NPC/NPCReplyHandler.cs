using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent when an NPC takes damage from a weapon
    /// </summary>
    [AutoMappedType]
    public class NPCReplyHandler : NPCTakeDamageHandler<NpcReplyServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Npc;

        public NPCReplyHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICurrentMapStateRepository currentMapStateRepository,
                               IEnumerable<INPCActionNotifier> npcNotifiers,
                               IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, npcNotifiers, otherCharacterAnimationNotifiers) { }

        public override bool HandlePacket(NpcReplyServerPacket packet)
        {
            // todo: npc kill steal protection
            Handle(packet.PlayerId, (EODirection)packet.PlayerDirection,
                packet.NpcIndex, packet.Damage, packet.HpPercentage);
            return true;
        }
    }
}