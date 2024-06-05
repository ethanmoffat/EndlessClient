using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.PacketHandlers.NPC;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Cast
{
    /// <summary>
    /// Sent when the main character levels up from killing an NPC with a spell
    /// </summary>
    [AutoMappedType]
    public class CastAcceptHandler : NPCDeathHandler<CastAcceptServerPacket>
    {
        private readonly IEnumerable<IEmoteNotifier> _emoteNotifiers;

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
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, characterSessionRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers, otherCharacterAnimationNotifiers)
        {
            _emoteNotifiers = emoteNotifiers;
        }

        public override bool HandlePacket(CastAcceptServerPacket packet)
        {
            DeathWorkflowSpell(packet.NpcKilledData, packet.Experience, Option.Some((packet.SpellId, packet.CasterTp)));
            ApplyStats(packet.LevelUp);

            foreach (var notifier in _emoteNotifiers)
                notifier.NotifyEmote(_characterRepository.MainCharacter.ID, Domain.Character.Emote.LevelUp);

            return true;
        }
    }
}
