using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Spell
{
    /// <summary>
    /// Sent when a player targets another player with a heal spell
    /// </summary>
    [AutoMappedType]
    public class SpellTargetOtherHandler : InGameOnlyPacketHandler<SpellTargetOtherServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _animationNotifiers;

        public override PacketFamily Family => PacketFamily.Spell;
        public override PacketAction Action => PacketAction.TargetOther;

        public SpellTargetOtherHandler(IPlayerInfoProvider playerInfoProvider,
                                       ICharacterRepository characterRepository,
                                       ICurrentMapStateRepository currentMapStateRepository,
                                       IEnumerable<IOtherCharacterAnimationNotifier> animationNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _animationNotifiers = animationNotifiers;
        }

        public override bool HandlePacket(SpellTargetOtherServerPacket packet)
        {
            if (packet.CasterId == _characterRepository.MainCharacter.ID)
            {
                var renderProps = _characterRepository.MainCharacter.RenderProperties.WithDirection((EODirection)packet.CasterDirection);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(renderProps);
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(packet.CasterId, out var character))
            {
                var updatedCharacter = character.WithRenderProperties(character.RenderProperties.WithDirection((EODirection)packet.CasterDirection));
                _currentMapStateRepository.Characters.Update(character, updatedCharacter);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(packet.CasterId);
                return true;
            }

            if (packet.Hp.HasValue)
            {
                var stats = _characterRepository.MainCharacter.Stats.WithNewStat(CharacterStat.HP, packet.Hp.Value);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
            }

            foreach (var notifier in _animationNotifiers)
                notifier.NotifyTargetOtherSpellCast(packet.CasterId, packet.VictimId, packet.SpellId, packet.SpellHealHp, packet.HpPercentage);

            return true;
        }
    }
}
