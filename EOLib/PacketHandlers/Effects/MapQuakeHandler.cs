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
    public class MapQuakeHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;
        public override PacketAction Action => PacketAction.Use;

        public MapQuakeHandler(IPlayerInfoProvider playerInfoProvider,
                               IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            const int EffectQuake = 1;

            if (packet.ReadChar() != EffectQuake)
                return false;

            var strength = packet.ReadChar();

            foreach (var notifier in _effectNotifiers)
                notifier.NotifyMapEffect(MapEffect.Quake1, strength);

            return true;
        }
    }
}
