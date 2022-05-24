using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class TimedSpikeEffectHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Report;

        public TimedSpikeEffectHandler(IPlayerInfoProvider playerInfoProvider,
                                       IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if ((char)packet.ReadByte() != 'S')
                return false;

            foreach (var notifier in _effectNotifiers)
                notifier.NotifyMapEffect(MapEffect.Spikes);

            return true;
        }
    }
}
