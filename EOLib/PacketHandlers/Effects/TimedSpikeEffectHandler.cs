using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class TimedSpikeEffectHandler : InGameOnlyPacketHandler<EffectReportServerPacket>
    {
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Report;

        public TimedSpikeEffectHandler(IPlayerInfoProvider playerInfoProvider,
                                       ICurrentMapProvider currentMapProvider,
                                       ICharacterProvider characterProvider,
                                       ICurrentMapStateRepository currentMapStateRepository,
                                       IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapProvider = currentMapProvider;
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(EffectReportServerPacket packet)
        {
            _currentMapStateRepository.LastTimedSpikeEvent = Option.Some(DateTime.Now);

            var characterPosition = _characterProvider.MainCharacter.RenderProperties.Coordinates();
            var distanceToSpikes = _currentMapProvider.CurrentMap.GetDistanceToClosestTileSpec(TileSpec.SpikesTimed, characterPosition);

            if (distanceToSpikes <= 6)
            {
                foreach (var notifier in _effectNotifiers)
                    notifier.NotifyMapEffect(IO.Map.MapEffect.Spikes);
            }

            return true;
        }
    }
}
