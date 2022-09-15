using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class EffectAgreeHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var effectId = packet.ReadShort();

            foreach (var notifier in _effectNotifiers)
                notifier.NotifyEffectAtLocation(x, y, effectId);

            return true;
        }
    }
}
