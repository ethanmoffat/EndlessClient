using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Recover
{
    /// <summary>
    /// Sent when the main character is given EXP or Karma in a quest
    /// </summary>
    [AutoMappedType]
    public class RecoverReplyHandler : InGameOnlyPacketHandler<RecoverReplyServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IEmoteNotifier> _emoteNotifiers;

        public override PacketFamily Family => PacketFamily.Recover;

        public override PacketAction Action => PacketAction.Reply;

        public RecoverReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICharacterRepository characterRepository,
                                   IEnumerable<IEmoteNotifier> emoteNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _emoteNotifiers = emoteNotifiers;
        }

        public override bool HandlePacket(RecoverReplyServerPacket packet)
        {
            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Experience, packet.Experience)
                .WithNewStat(CharacterStat.Karma, packet.Karma);

            if (packet.LevelUp.HasValue && packet.LevelUp > 0)
            {
                stats = stats.WithNewStat(CharacterStat.Level, packet.LevelUp.Value);
                foreach (var notifier in _emoteNotifiers)
                    notifier.NotifyEmote(_characterRepository.MainCharacter.ID, Domain.Character.Emote.LevelUp);
            }

            if (packet.StatPoints.HasValue)
            {
                stats = stats.WithNewStat(CharacterStat.StatPoints, packet.StatPoints.Value);
            }

            if (packet.SkillPoints.HasValue)
            {
                stats = stats.WithNewStat(CharacterStat.SkillPoints, packet.SkillPoints.Value);
            }

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }
}
