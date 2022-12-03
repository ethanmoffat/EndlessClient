using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.PacketHandlers.NPC;

namespace EOLib.PacketHandlers.Cast
{
    /// <summary>
    /// Sent when the main character levels up from killing an NPC with a spell
    /// </summary>
    [AutoMappedType]
    public class CastAcceptHandler : NPCAcceptHandler
    {
        public override PacketFamily Family => PacketFamily.Cast;

        public override PacketAction Action => PacketAction.Accept;

        public CastAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 ICharacterRepository characterRepository,
                                 ICharacterSessionRepository characterSessionRepository,
                                 IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
                                 IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                 IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers,
                                 IEnumerable<IEmoteNotifier> emoteNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, characterSessionRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers, otherCharacterAnimationNotifiers, emoteNotifiers)
        { }
    }
}
