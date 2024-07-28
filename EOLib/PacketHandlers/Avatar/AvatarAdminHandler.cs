using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Avatar
{
    /// <summary>
    /// Sent when a player takes damge from a PK spell
    /// </summary>
    [AutoMappedType]
    public class AvatarAdminHandler : InGameOnlyPacketHandler<AvatarAdminServerPacket>
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

        public override bool HandlePacket(AvatarAdminServerPacket packet)
        {
            var sourcePlayerId = packet.CasterId;
            var targetPlayerId = packet.VictimId;
            var damage = packet.Damage;
            var sourcePlayerDirection = (EODirection)packet.CasterDirection;
            var targetPercentHealth = packet.HpPercentage;
            var targetIsDead = packet.VictimDied;
            var spellId = packet.SpellId;

            if (sourcePlayerId == _characterRepository.MainCharacter.ID)
            {
                var renderProps = _characterRepository.MainCharacter.RenderProperties.WithDirection(sourcePlayerDirection);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(renderProps);
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(sourcePlayerId, out var sourceCharacter))
            {
                var updatedCharacter = sourceCharacter.WithRenderProperties(sourceCharacter.RenderProperties.WithDirection(sourcePlayerDirection));
                _currentMapStateRepository.Characters.Update(sourceCharacter, updatedCharacter);
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
            else if (_currentMapStateRepository.Characters.TryGetValue(targetPlayerId, out var targetCharacter))
            {
                var renderProps = targetCharacter.RenderProperties.WithIsDead(targetIsDead);

                var stats = targetCharacter.Stats;
                stats = stats.WithNewStat(CharacterStat.HP, stats[CharacterStat.HP] - damage);

                _currentMapStateRepository.Characters.Update(targetCharacter, targetCharacter.WithStats(stats).WithRenderProperties(renderProps));
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