using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Avatar
{
    /// <summary>
    /// Sent when a player takes damge from a PK spell
    /// </summary>
    [AutoMappedType]
    public class AvatarAdminHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _animationNotifiers;

        public override PacketFamily Family => PacketFamily.Avatar;

        public override PacketAction Action => PacketAction.Admin;

        public AvatarAdminHandler(IPlayerInfoProvider playerInfoProvider,
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
            var sourcePlayerId = packet.ReadShort();
            var targetPlayerId = packet.ReadShort();
            var damage = packet.ReadThree();
            var sourcePlayerDirection = (EODirection)packet.ReadChar();
            var targetPercentHealth = packet.ReadChar();
            var targetIsDead = packet.ReadChar() != 0;
            var spellId = packet.ReadShort();

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
                _currentMapStateRepository.UnknownPlayerIDs.Add(sourcePlayerId);
                return true;
            }

            if (targetPlayerId == _characterRepository.MainCharacter.ID)
            {
                var renderProps = _characterRepository.MainCharacter.RenderProperties
                    .WithIsDead(targetIsDead);

                var stats = _characterRepository.MainCharacter.Stats;
                stats = stats.WithNewStat(CharacterStat.HP, stats[CharacterStat.HP] - damage);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter
                    .WithStats(stats)
                    .WithRenderProperties(renderProps);
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(targetPlayerId))
            {
                var c = _currentMapStateRepository.Characters[targetPlayerId];

                var renderProps = c.RenderProperties.WithIsDead(targetIsDead);

                var stats = c.Stats;
                stats = stats.WithNewStat(CharacterStat.HP, stats[CharacterStat.HP] - damage);

                _currentMapStateRepository.Characters[targetPlayerId] = c.WithStats(stats).WithRenderProperties(renderProps);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(targetPlayerId);
                return true;
            }

            foreach (var notifier in _animationNotifiers)
                notifier.NotifyTargetOtherSpellCast(sourcePlayerId, targetPlayerId, spellId, damage, targetPercentHealth);

            return true;
        }
    }
}
