using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class MapQuakeHandler : InGameOnlyPacketHandler<EffectUseServerPacket>
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

        public override bool HandlePacket(EffectUseServerPacket packet)
        {
            if (packet.Effect != MapEffect.Quake)
                return false;

            foreach (var notifier in _effectNotifiers)
                notifier.NotifyMapEffect(IO.Map.MapEffect.Quake1, ((EffectUseServerPacket.EffectDataQuake)packet.EffectData).QuakeStrength);

            return true;
        }
    }
}