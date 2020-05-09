using System;
using System.Collections.Generic;
using System.Linq;
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
            var dir = (EODirection)packet.ReadChar();
            var x = packet.ReadChar();
            var y = packet.ReadChar();

            ICharacter character;
            try
            {
                character = _currentMapStateRepository.Characters.Single(cc => cc.ID == characterID);
            }
            catch (InvalidOperationException) { return false; }

            var renderProperties = character.RenderProperties.WithDirection(dir);
            renderProperties = EnsureCorrectXAndY(renderProperties, x, y);
            var newCharacter = character.WithRenderProperties(renderProperties);

            _currentMapStateRepository.Characters.Remove(character);
            _currentMapStateRepository.Characters.Add(newCharacter);

            foreach (var notifier in _otherCharacterAnimationNotifiers)
                notifier.StartOtherCharacterWalkAnimation(characterID);

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
