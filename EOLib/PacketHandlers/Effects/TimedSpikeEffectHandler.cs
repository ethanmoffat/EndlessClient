using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class TimedSpikeEffectHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Report;

        public TimedSpikeEffectHandler(IPlayerInfoProvider playerInfoProvider,
                                       ICurrentMapProvider currentMapProvider,
                                       ICharacterProvider characterProvider,
                                       IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapProvider = currentMapProvider;
            _characterProvider = characterProvider;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if ((char)packet.ReadByte() != 'S')
                return false;

            var characterPosition = _characterProvider.MainCharacter.RenderProperties.Coordinates();
            var distanceToSpikes = _currentMapProvider.CurrentMap.GetDistanceToClosestTileSpec(TileSpec.SpikesTimed, characterPosition);

            if (distanceToSpikes <= 6)
            {
                foreach (var notifier in _effectNotifiers)
                    notifier.NotifyMapEffect(MapEffect.Spikes);
            }

            return true;
        }
    }
}
