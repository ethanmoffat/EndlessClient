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
    public class EffectPotionHandler : InGameOnlyPacketHandler<EffectPlayerServerPacket>
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

        public override bool HandlePacket(EffectPlayerServerPacket packet)
        {
            foreach (var effect in packet.Effects)
            {
                foreach (var notifier in _effectNotifiers)
                    notifier.NotifyPotionEffect(effect.PlayerId, effect.EffectId);
            }

            return true;
        }
    }
}