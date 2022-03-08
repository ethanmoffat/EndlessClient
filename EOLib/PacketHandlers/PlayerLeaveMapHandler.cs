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
    public class PlayerLeaveMapHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Avatar;

        public override PacketAction Action => PacketAction.Remove;

        public PlayerLeaveMapHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository currentMapStateRepository,
                                     IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();

            var anim = WarpAnimation.None;
            if (packet.ReadPosition < packet.Length)
            {
                anim = (WarpAnimation)packet.ReadChar();
                foreach (var notifier in _effectNotifiers)
                    notifier.NotifyWarpLeaveEffect(id, anim);
            }

            if (!_currentMapStateRepository.Characters.ContainsKey(id))
                return false;

            var character = _currentMapStateRepository.Characters[id];
            _currentMapStateRepository.Characters.Remove(id);
            _currentMapStateRepository.VisibleSpikeTraps.Remove(new MapCoordinate(character.RenderProperties.MapX, character.RenderProperties.MapY));

            return true;
        }
    }
}
