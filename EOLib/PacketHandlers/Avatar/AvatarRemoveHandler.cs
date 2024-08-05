using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Avatar
{
    /// <summary>
    /// Sent when a player leaves the map
    /// </summary>
    [AutoMappedType]
    public class AvatarRemoveHandler : InGameOnlyPacketHandler<AvatarRemoveServerPacket>
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

        public override bool HandlePacket(AvatarRemoveServerPacket packet)
        {
            if (packet.WarpEffect != null)
            {
                foreach (var notifier in _effectNotifiers)
                    notifier.NotifyWarpLeaveEffect(packet.PlayerId, packet.WarpEffect.Value);
            }

            if (_characterRepository.MainCharacter.ID == packet.PlayerId)
            {
                _characterRepository.HasAvatar = false;
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(packet.PlayerId, out var character))
            {
                _currentMapStateRepository.Characters.Remove(character);
            }

            return true;
        }
    }
}