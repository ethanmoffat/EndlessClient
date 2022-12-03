using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.PacketHandlers.NPC;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Cast
{
    /// <summary>
    /// Sent when an NPC takes damage from a spell cast
    /// </summary>
    [AutoMappedType]
    public class CastReplyHandler : NPCTakeDamageHandler
    {
        public override PacketFamily Family => PacketFamily.Cast;

        public CastReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository,
                                IEnumerable<INPCActionNotifier> npcNotifiers,
                                IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, npcNotifiers, otherCharacterAnimationNotifiers) { }
    }
}
