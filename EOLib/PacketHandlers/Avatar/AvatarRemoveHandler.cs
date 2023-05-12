using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Avatar
{
    /// <summary>
    /// Sent when a player leaves the map
    /// </summary>
    [AutoMappedType]
    public class AvatarRemoveHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Avatar;

        public override PacketAction Action => PacketAction.Remove;

        public AvatarRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICharacterRepository characterRepository,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();

            if (packet.ReadPosition < packet.Length)
            {
                var anim = (WarpAnimation)packet.ReadChar();
                foreach (var notifier in _effectNotifiers)
                    notifier.NotifyWarpLeaveEffect(id, anim);
            }

            if (_characterRepository.MainCharacter.ID == id)
            {
                _characterRepository.HasAvatar = false;
                _currentMapStateRepository.VisibleSpikeTraps.Remove(_characterRepository.MainCharacter.RenderProperties.Coordinates());
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(id, out var character))
            {
                _currentMapStateRepository.Characters.Remove(character);
                _currentMapStateRepository.VisibleSpikeTraps.Remove(character.RenderProperties.Coordinates());
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(id);
            }

            return true;
        }
    }
}
