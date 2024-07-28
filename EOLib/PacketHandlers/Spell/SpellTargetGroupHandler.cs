using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Domain.Spells;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Spell
{
    /// <summary>
    /// Sent when a spell targeting a group is successfully cast
    /// </summary>
    [AutoMappedType]
    public class SpellTargetGroupHandler : InGameOnlyPacketHandler<SpellTargetGroupServerPacket>
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

        public override bool HandlePacket(SpellTargetGroupServerPacket packet)
        {
            if (packet.CasterId == _characterRepository.MainCharacter.ID)
            {
                var stats = _characterRepository.MainCharacter.Stats.WithNewStat(CharacterStat.TP, packet.CasterTp);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
            }

            var spellTargets = packet.Players
                .Select(x => new GroupSpellTarget.Builder
                {
                    TargetId = x.PlayerId,
                    TargetHp = x.Hp,
                    PercentHealth = x.HpPercentage,
                }.ToImmutable())
                .ToList();

            // todo: eoserv potentially sends garbage 255 bytes in packet.Players
            foreach (var target in spellTargets)
            {
                _partyDataRepository.Members.SingleOrNone(x => x.CharacterID == target.TargetId)
                    .MatchSome(x =>
                    {
                        _partyDataRepository.Members.Remove(x);
                        _partyDataRepository.Members.Add(x.WithPercentHealth(target.PercentHealth));
                    });

                if (target.TargetId == _characterRepository.MainCharacter.ID)
                {
                    var stats = _characterRepository.MainCharacter.Stats.WithNewStat(CharacterStat.HP, target.TargetHp);
                    _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
                }
            }

            foreach (var notifier in _notifiers)
            {
                notifier.NotifyGroupSpellCast(packet.CasterId, packet.SpellId, packet.SpellHealHp, spellTargets);
            }

            return true;
        }
    }
}