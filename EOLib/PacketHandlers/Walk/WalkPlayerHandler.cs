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

namespace EOLib.PacketHandlers.Walk
{
    /// <summary>
    /// Sent in response to another player walking successfully
    /// </summary>
    [AutoMappedType]
    public class WalkPlayerHandler : InGameOnlyPacketHandler<WalkPlayerServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        public override PacketFamily Family => PacketFamily.Walk;

        public override PacketAction Action => PacketAction.Player;

        public WalkPlayerHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        public override bool HandlePacket(WalkPlayerServerPacket packet)
        {

            if (_currentMapStateRepository.Characters.TryGetValue(packet.PlayerId, out var character))
            {

                // if character is walking, that means animator is handling position of character
                // if character is not walking (this is true in EOBot), update the domain model here
                if (!character.RenderProperties.IsActing(CharacterActionState.Walking))
                {
                    var renderProperties = EnsureCorrectXAndY(character.RenderProperties.WithDirection((EODirection)packet.Direction), packet.Coords.X, packet.Coords.Y);
                    _currentMapStateRepository.Characters.Update(character, character.WithRenderProperties(renderProperties));
                }

                foreach (var notifier in _otherCharacterAnimationNotifiers)
                    notifier.StartOtherCharacterWalkAnimation(packet.PlayerId, new MapCoordinate(packet.Coords.X, packet.Coords.Y), (EODirection)packet.Direction);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
            }

            return true;
        }

        private static CharacterRenderProperties EnsureCorrectXAndY(CharacterRenderProperties renderProperties, int x, int y)
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
