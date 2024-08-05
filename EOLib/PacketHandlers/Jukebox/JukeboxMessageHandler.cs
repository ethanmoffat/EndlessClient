using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Jukebox
{
    [AutoMappedType]
    public class JukeboxMessageHandler : InGameOnlyPacketHandler<JukeboxMsgServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _otherCharacterAnimationNotifiers;

        public override PacketFamily Family => PacketFamily.Jukebox;

        public override PacketAction Action => PacketAction.Msg;

        public JukeboxMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository currentMapStateRepository,
                                     IEnumerable<IOtherCharacterAnimationNotifier> otherCharacterAnimationNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _otherCharacterAnimationNotifiers = otherCharacterAnimationNotifiers;
        }

        public override bool HandlePacket(JukeboxMsgServerPacket packet)
        {
            if (_currentMapStateRepository.Characters.TryGetValue(packet.PlayerId, out var character))
            {
                if (character.RenderProperties.WeaponGraphic == packet.InstrumentId)
                {
                    var updatedCharacter = character.WithRenderProperties(character.RenderProperties.WithDirection((EODirection)packet.Direction));
                    _currentMapStateRepository.Characters.Update(character, updatedCharacter);

                    foreach (var notifier in _otherCharacterAnimationNotifiers)
                        notifier.StartOtherCharacterAttackAnimation(packet.PlayerId, packet.NoteId - 1);
                }
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
            }

            return true;
        }
    }
}