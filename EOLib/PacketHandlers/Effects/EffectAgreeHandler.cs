using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class EffectAgreeHandler : InGameOnlyPacketHandler<EffectAgreeServerPacket>
    {
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;
        public override PacketAction Action => PacketAction.Agree;

        public EffectAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                  IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(EffectAgreeServerPacket packet)
        {
            foreach (var effect in packet.Effects)
            {
                foreach (var notifier in _effectNotifiers)
                    notifier.NotifyEffectAtLocation(new MapCoordinate(effect.Coords.X, effect.Coords.Y), effect.EffectId);
            }

            return true;
        }
    }
}
