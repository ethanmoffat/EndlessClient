using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Attack
{
    /// <summary>
    /// Sent when another player attacks
    /// </summary>
    [AutoMappedType]
    public class PlayerAttackHandler : InGameOnlyPacketHandler<AttackPlayerServerPacket>
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

        public override bool HandlePacket(AttackPlayerServerPacket packet)
        {
            if (_currentMapStateRepository.Characters.TryGetValue(packet.PlayerId, out var character))
            {
                if (character.RenderProperties.Direction != (EODirection)packet.Direction)
                {
                    var renderProperties = character.RenderProperties.WithDirection((EODirection)packet.Direction);
                    _currentMapStateRepository.Characters.Update(character, character.WithRenderProperties(renderProperties));
                }

                foreach (var notifier in _otherCharacterAnimationNotifiers)
                    notifier.StartOtherCharacterAttackAnimation(packet.PlayerId);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
            }

            return true;
        }
    }
}
