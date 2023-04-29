using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Domain.Spells;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Spell
{
    /// <summary>
    /// Sent when a spell targeting a group is successfully cast
    /// </summary>
    [AutoMappedType]
    public class SpellTargetGroupHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IPartyDataRepository _partyDataRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _notifiers;

        public override PacketFamily Family => PacketFamily.Spell;

        public override PacketAction Action => PacketAction.TargetGroup;

        public SpellTargetGroupHandler(IPlayerInfoProvider playerInfoProvider,
                                       ICharacterRepository characterRepository,
                                       IPartyDataRepository partyDataRepository,
                                       IEnumerable<IOtherCharacterAnimationNotifier> notifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _partyDataRepository = partyDataRepository;
            _notifiers = notifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var spellId = packet.ReadShort();
            var sourcePlayerId = packet.ReadShort();
            var fromPlayerTp = packet.ReadShort();
            var spellHealthGain = packet.ReadShort();

            if (sourcePlayerId == _characterRepository.MainCharacter.ID)
            {
                var stats = _characterRepository.MainCharacter.Stats.WithNewStat(CharacterStat.TP, fromPlayerTp);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
            }

            var spellTargets = new List<GroupSpellTarget>();
            while (packet.ReadPosition != packet.Length)
            {
                // eoserv puts 5 '255' bytes between party members
                // unknown what data structures are supposed to be represented between these break bytes
                // todo: these bytes are garbage data for an empty record (5 bytes read per group spell target) - handle accordingly
                if (packet.ReadBytes(5).Any(x => x != 255)) return false;

                var targetId = packet.ReadShort();
                var targetPercentHealth = packet.ReadChar();
                var targetHp = packet.ReadShort();

                spellTargets.Add(new GroupSpellTarget.Builder
                {
                    TargetId = targetId,
                    PercentHealth = targetPercentHealth,
                    TargetHp = targetHp,
                }.ToImmutable());

                _partyDataRepository.Members.SingleOrNone(x => x.CharacterID == targetId)
                    .MatchSome(x =>
                    {
                        _partyDataRepository.Members.Remove(x);
                        _partyDataRepository.Members.Add(x.WithPercentHealth(targetPercentHealth));
                    });

                if (targetId == _characterRepository.MainCharacter.ID)
                {
                    var stats = _characterRepository.MainCharacter.Stats.WithNewStat(CharacterStat.HP, targetHp);
                    _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
                }
            }

            foreach (var notifier in _notifiers)
            {
                notifier.NotifyGroupSpellCast(sourcePlayerId, spellId, spellHealthGain, spellTargets);
            }

            return true;
        }
    }
}
