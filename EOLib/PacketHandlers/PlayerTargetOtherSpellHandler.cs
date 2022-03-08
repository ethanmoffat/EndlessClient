using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class PlayerTargetOtherSpellHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _animationNotifiers;

        public override PacketFamily Family => PacketFamily.Spell;
        public override PacketAction Action => PacketAction.TargetOther;

        public PlayerTargetOtherSpellHandler(IPlayerInfoProvider playerInfoProvider,
                                             ICharacterRepository characterRepository,
                                             ICurrentMapStateRepository currentMapStateRepository,
                                             IEnumerable<IOtherCharacterAnimationNotifier> animationNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _animationNotifiers = animationNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var targetPlayerId = packet.ReadShort();
            var sourcePlayerId = packet.ReadShort();
            var sourcePlayerDirection = (EODirection)packet.ReadChar();
            var spellId = packet.ReadShort();
            var recoveredHP = packet.ReadInt();
            var targetPercentHealth = packet.ReadChar();

            if (sourcePlayerId == _characterRepository.MainCharacter.ID)
            {
                var renderProps = _characterRepository.MainCharacter.RenderProperties.WithDirection(sourcePlayerDirection);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(renderProps);
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(sourcePlayerId))
            {
                var character = _currentMapStateRepository.Characters[sourcePlayerId];
                var updatedCharacter = character.WithRenderProperties(character.RenderProperties.WithDirection(sourcePlayerDirection));
                _currentMapStateRepository.Characters[sourcePlayerId] = updatedCharacter;
            }
            else
            {
                return false;
            }

            if (packet.ReadPosition != packet.Length)
            {
                var targetPlayerCurrentHp = packet.ReadShort();
                var stats = _characterRepository.MainCharacter.Stats.WithNewStat(CharacterStat.HP, targetPlayerCurrentHp);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
            }

            foreach (var notifier in _animationNotifiers)
                notifier.NotifyTargetOtherSpellCast(sourcePlayerId, targetPlayerId, spellId, recoveredHP, targetPercentHealth);

            return true;
        }
    }
}
