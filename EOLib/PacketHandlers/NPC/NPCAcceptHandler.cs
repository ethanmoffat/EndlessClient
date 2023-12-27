using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent when the main character levels up from killing an NPC
    /// </summary>
    [AutoMappedType]
    public class NPCAcceptHandler : NPCSpecHandler
    {
        private readonly IEnumerable<IEmoteNotifier> _emoteNotifiers;

        public override PacketFamily Family => PacketFamily.NPC;

        public override PacketAction Action => PacketAction.Accept;

        public NPCAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                ICurrentMapStateRepository currentMapStateRepository,
                                ICharacterRepository characterRepository,
                                ICharacterSessionRepository characterSessionRepository,
                                IEnumerable<INPCActionNotifier> npcAnimationNotifiers,
                                IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers,
                                IEnumerable<IEmoteNotifier> emoteNotifiers)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, characterSessionRepository,
                   npcAnimationNotifiers, mainCharacterEventNotifiers, otherCharacterAnimationNotifiers)
        {
            _emoteNotifiers = emoteNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if (!base.HandlePacket(packet))
                return false;

            var level = packet.ReadChar();
            var stat = packet.ReadShort();
            var skill = packet.ReadShort();
            var maxhp = packet.ReadShort();
            var maxtp = packet.ReadShort();
            var maxsp = packet.ReadShort();

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Level, level)
                .WithNewStat(CharacterStat.StatPoints, stat)
                .WithNewStat(CharacterStat.SkillPoints, skill)
                .WithNewStat(CharacterStat.MaxHP, maxhp)
                .WithNewStat(CharacterStat.MaxTP, maxtp)
                .WithNewStat(CharacterStat.MaxSP, maxsp);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            foreach (var notifier in _emoteNotifiers)
                notifier.NotifyEmote(_characterRepository.MainCharacter.ID, Domain.Character.Emote.LevelUp);

            return true;
        }
    }
}
