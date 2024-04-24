using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Domain.Login;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Avatar
{
    [AutoMappedType]
    public class AvatarReplyHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Avatar;
        public override PacketAction Action => PacketAction.Reply;

        public AvatarReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICurrentMapStateRepository currentStateRepository,
                                  IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers,
                                  IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers,
                                  IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                  ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _currentStateRepository = currentStateRepository;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var attackerId = packet.ReadShort();
            var victimId = packet.ReadShort();
            var damage = packet.ReadThree();
            var direction = (EODirection)packet.ReadChar();
            var hpPercentage = packet.ReadChar();
            bool dead = packet.ReadChar() != 0;

            if (victimId == _characterRepository.MainCharacter.ID)
            {
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithDamage(damage, dead);

                foreach (var notifier in _mainCharacterEventNotifiers)
                {
                    notifier.NotifyTakeDamage(damage, hpPercentage, isHeal: false);
                }
            }
            else if (_currentStateRepository.Characters.TryGetValue(victimId, out var character))
            {
                var updatedCharacter = character.WithDamage(damage, dead);
                _currentStateRepository.Characters.Update(character, updatedCharacter);

                foreach (var notifier in _otherCharacterEventNotifiers)
                {
                    notifier.OtherCharacterTakeDamage(victimId, hpPercentage, damage, isHeal: false);
                }
            }
            if (attackerId != _characterRepository.MainCharacter.ID)
            {
                foreach (var notifier in _otherCharacterAnimationNotifiers)
                {
                    notifier.StartOtherCharacterAttackAnimation(attackerId);
                }   
            }

            return true;
        }
    }
}
