using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Spell
{
    /// <summary>
    /// Sent when a player cast a spell targeting themselves
    /// </summary>
    [AutoMappedType]
    public class SpellTargetSelfHandler : InGameOnlyPacketHandler<SpellTargetSelfServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _animationNotifiers;

        public override PacketFamily Family => PacketFamily.Spell;
        public override PacketAction Action => PacketAction.TargetSelf;

        public SpellTargetSelfHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICharacterRepository characterRepository,
                                      IEnumerable<IOtherCharacterAnimationNotifier> animationNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _animationNotifiers = animationNotifiers;
        }

        public override bool HandlePacket(SpellTargetSelfServerPacket packet)
        {
            var fromPlayerID = packet.PlayerId;
            var spellID = packet.SpellId;
            var spellHP = packet.SpellHealHp;
            var percentHealth = packet.HpPercentage;

            if (packet.ByteSize > 12)
            {
                var stats = _characterRepository.MainCharacter.Stats;
                stats = stats.WithNewStat(CharacterStat.HP, packet.Hp.Value)
                             .WithNewStat(CharacterStat.TP, packet.Tp.Value);

                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
            }

            foreach (var notifier in _animationNotifiers)
                notifier.NotifySelfSpellCast(fromPlayerID, spellID, spellHP, percentHealth);

            return true;
        }
    }
}
