using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class EffectPotionHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;
        public override PacketAction Action => PacketAction.Player;

        public EffectPotionHandler(IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerId = packet.ReadShort();
            var effectId = packet.ReadThree();

            foreach (var notifier in _effectNotifiers)
                notifier.NotifyPotionEffect(playerId, effectId);

            return true;
        }
    }
}
