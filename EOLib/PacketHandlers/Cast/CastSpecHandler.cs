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
    /// Sent when an NPC dies to a spell
    ///
    /// This is handled the same way as the NPC_SPEC packet. There is some additional special handling 
    /// that is done from NPCLeaveMapHandler.HandlePlacket (see if packet.Family == PacketFamily.Cast) blocks
    /// </summary>
    [AutoMappedType]
    public class CastSpecHandler : NPCSpecHandler
    {
        public override PacketFamily Family => PacketFamily.Cast;

        public override PacketAction Action => PacketAction.Spec;

        public CastSpecHandler(IPlayerInfoProvider playerInfoProvider,
                               ICurrentMapStateRepository currentMapStateRepository,
                               ICharacterRepository characterRepository,
                               ICharacterSessionRepository characterSessionRepository,
                               IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
                               IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                               IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, characterSessionRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers, otherCharacterAnimationNotifiers)
        { }
    }
}
