using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Avatar
{
    [AutoMappedType]
    public class AvatarReplyHandler : InGameOnlyPacketHandler<AvatarReplyServerPacket>
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

        public override bool HandlePacket(AvatarReplyServerPacket packet)
        {
            var direction = (EODirection)packet.Direction;

            if (packet.VictimId == _characterRepository.MainCharacter.ID)
            {
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithDamage(packet.Damage, packet.Dead);

                foreach (var notifier in _mainCharacterEventNotifiers)
                {
                    notifier.NotifyTakeDamage(packet.Damage, packet.HpPercentage, isHeal: false);
                }
            }
            else if (_currentStateRepository.Characters.TryGetValue(packet.VictimId, out var character))
            {
                var updatedCharacter = character.WithDamage(packet.Damage, packet.Dead);
                _currentStateRepository.Characters.Update(character, updatedCharacter);

                foreach (var notifier in _otherCharacterEventNotifiers)
                {
                    notifier.OtherCharacterTakeDamage(packet.VictimId, packet.HpPercentage, packet.Damage, isHeal: false);
                }
            }

            if (packet.PlayerId == _characterRepository.MainCharacter.ID)
            {
                _characterRepository.MainCharacter = _characterRepository.MainCharacter
                    .WithRenderProperties(_characterRepository.MainCharacter.RenderProperties.WithDirection(direction));
            }
            else if (_currentStateRepository.Characters.TryGetValue(packet.PlayerId, out var otherCharacter))
            {
                _currentStateRepository.Characters.Update(
                    otherCharacter,
                    otherCharacter.WithRenderProperties(otherCharacter.RenderProperties.WithDirection(direction))
                );
                foreach (var notifier in _otherCharacterAnimationNotifiers)
                {
                    notifier.StartOtherCharacterAttackAnimation(packet.PlayerId);
                }
            }
            else
            {
                _currentStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
            }

            return true;
        }
    }
}