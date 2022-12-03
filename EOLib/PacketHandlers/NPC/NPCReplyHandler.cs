using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent when an NPC takes damage from a weapon
    /// </summary>
    [AutoMappedType]
    public class NPCReplyHandler : NPCTakeDamageHandler
    {
        public override PacketFamily Family => PacketFamily.NPC;

        public NPCReplyHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICurrentMapStateRepository currentMapStateRepository,
                               IEnumerable<INPCActionNotifier> npcNotifiers,
                               IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, npcNotifiers, otherCharacterAnimationNotifiers) { }
    }
}
