using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Jukebox
{
    [AutoMappedType]
    public class JukeboxMessageHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        public override PacketFamily Family => PacketFamily.JukeBox;

        public override PacketAction Action => PacketAction.Message;

        public JukeboxMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository currentMapStateRepository,
                                     IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerId = packet.ReadShort();
            var direction = (EODirection)packet.ReadChar();
            var instrument = packet.ReadChar();
            var note = packet.ReadChar();

            if (_currentMapStateRepository.Characters.TryGetValue(playerId, out var character))
            {
                if (character.RenderProperties.WeaponGraphic == instrument)
                {
                    var updatedCharacter = character.WithRenderProperties(character.RenderProperties.WithDirection(direction));
                    _currentMapStateRepository.Characters.Update(character, updatedCharacter);

                    foreach (var notifier in _otherCharacterAnimationNotifiers)
                        notifier.StartOtherCharacterAttackAnimation(playerId, note - 1);
                }
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerId);
            }

            return true;
        }
    }
}
