using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class PlayerAttackHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        public override PacketFamily Family => PacketFamily.Attack;

        public override PacketAction Action => PacketAction.Player;

        public PlayerAttackHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerID = packet.ReadShort();
            var direction = (EODirection)packet.ReadChar();

            if (!_currentMapStateRepository.Characters.ContainsKey(playerID))
                return false;

            var character = _currentMapStateRepository.Characters[playerID];
            if (character.RenderProperties.Direction != direction)
            {
                var renderProperties = character.RenderProperties.WithDirection(direction);
                _currentMapStateRepository.Characters[playerID] = character.WithRenderProperties(renderProperties);
            }

            foreach (var notifier in _otherCharacterAnimationNotifiers)
                notifier.StartOtherCharacterAttackAnimation(playerID);

            return true;
        }
    }
}
