using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class RecoverReplyHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var exp = packet.ReadInt();
            var karma = packet.ReadShort();
            var level = packet.ReadChar();

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Experience, exp)
                .WithNewStat(CharacterStat.Karma, karma);

            if (level > 0)
            {
                stats = stats.WithNewStat(CharacterStat.Level, level);
                foreach (var notifier in _emoteNotifiers)
                    notifier.NotifyEmote((short)_characterRepository.MainCharacter.ID, Domain.Character.Emote.LevelUp);
            }

            if (packet.ReadPosition < packet.Length)
            {
                stats = stats.WithNewStat(CharacterStat.StatPoints, packet.ReadShort())
                    .WithNewStat(CharacterStat.SkillPoints, packet.ReadShort());
            }

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }
}
