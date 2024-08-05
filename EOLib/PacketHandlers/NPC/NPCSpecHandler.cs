using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent when an NPC dies to a weapon
    /// </summary>
    [AutoMappedType]
    public class NPCSpecHandler : NPCDeathHandler<NpcSpecServerPacket>
    {
        public override PacketFamily Family => PacketFamily.Npc;

        public override PacketAction Action => PacketAction.Spec;

        public NPCSpecHandler(IPlayerInfoProvider playerInfoProvider,
                              ICharacterRepository characterRepository,
                              ICurrentMapStateRepository currentMapStateRepository,
                              ICharacterSessionRepository characterSessionRepository,
                              IEnumerable<INPCActionNotifier> npcActionNotifiers,
                              IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                              IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, characterSessionRepository,
                  npcActionNotifiers, mainCharacterEventNotifiers, otherCharacterAnimationNotifiers)
        { }

        public override bool HandlePacket(NpcSpecServerPacket packet)
        {
            DeathWorkflow(packet.NpcKilledData, packet.Experience);
            return true;
        }
    }
}