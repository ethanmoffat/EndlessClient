using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class PlayerWalkHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        public override PacketFamily Family => PacketFamily.Walk;

        public override PacketAction Action => PacketAction.Player;

        public PlayerWalkHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var characterID = packet.ReadShort();

            if (_currentMapStateRepository.Characters.ContainsKey(characterID))
            {
                var dir = (EODirection)packet.ReadChar();
                var x = packet.ReadChar();
                var y = packet.ReadChar();

                var character = _currentMapStateRepository.Characters[characterID];

                // if character is walking, that means animator is handling position of character
                // if character is not walking (this is true in EOBot), update the domain model here
                if (!character.RenderProperties.IsActing(CharacterActionState.Walking))
                {
                    var renderProperties = EnsureCorrectXAndY(character.RenderProperties.WithDirection(dir), x, y);
                    _currentMapStateRepository.Characters[characterID] = character.WithRenderProperties(renderProperties);
                }

                foreach (var notifier in _otherCharacterAnimationNotifiers)
                    notifier.StartOtherCharacterWalkAnimation(characterID, x, y, dir);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(characterID);
            }

            return true;
        }

        private static ICharacterRenderProperties EnsureCorrectXAndY(ICharacterRenderProperties renderProperties, byte x, byte y)
        {
            var opposite = renderProperties.Direction.Opposite();
            var tempRenderProperties = renderProperties
                .WithDirection(opposite)
                .WithMapX(x)
                .WithMapY(y);
            return renderProperties
                .WithMapX(tempRenderProperties.GetDestinationX())
                .WithMapY(tempRenderProperties.GetDestinationY());
        }
    }
}
