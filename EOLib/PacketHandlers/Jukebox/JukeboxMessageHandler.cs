using AutomaticTypeMapper;
using EOLib.Domain.Character;
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
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
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

            if (_currentMapStateRepository.Characters.ContainsKey(playerId))
            {
                var c = _currentMapStateRepository.Characters[playerId];

                if (c.RenderProperties.WeaponGraphic == instrument)
                {
                    c = c.WithRenderProperties(c.RenderProperties.WithDirection(direction));
                    _currentMapStateRepository.Characters[playerId] = c;

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
