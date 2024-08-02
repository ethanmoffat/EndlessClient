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
    /// Sent when the main character levels up from killing an NPC
    /// </summary>
    [AutoMappedType]
    public class NPCAcceptHandler : NPCDeathHandler<NpcAcceptServerPacket>
    {
        private readonly IEnumerable<IEmoteNotifier> _emoteNotifiers;

        public override PacketFamily Family => PacketFamily.Npc;

        public override PacketAction Action => PacketAction.Accept;

        public NPCAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository,
                                ICharacterSessionRepository characterSessionRepository,
                                IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
                                IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers,
                                IEnumerable<IEmoteNotifier> emoteNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, characterSessionRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers, otherCharacterAnimationNotifiers)
        {
            _emoteNotifiers = emoteNotifiers;
        }

        public override bool HandlePacket(NpcAcceptServerPacket packet)
        {
            DeathWorkflow(packet.NpcKilledData, packet.Experience);
            ApplyStats(packet.LevelUp);

            foreach (var notifier in _emoteNotifiers)
                notifier.NotifyEmote(_characterRepository.MainCharacter.ID, Domain.Character.Emote.LevelUp);

            return true;
        }
    }
}