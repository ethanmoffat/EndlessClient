using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;

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

            ICharacter character;
            try
            {
                character = _currentMapStateRepository.Characters.Single(x => x.ID == id);
            }
            catch (InvalidOperationException) { return false; }

            _currentMapStateRepository.Characters.Remove(character);
            _currentMapStateRepository.VisibleSpikeTraps.Remove(new MapCoordinate(character.RenderProperties.MapX, character.RenderProperties.MapY));

            return true;
        }
    }
}
