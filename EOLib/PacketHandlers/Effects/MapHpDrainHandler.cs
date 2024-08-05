
using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class MapHpDrainHandler : InGameOnlyPacketHandler<EffectTargetOtherServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.TargetOther;

        public MapHpDrainHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                 IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers,
                                 IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(EffectTargetOtherServerPacket packet)
        {
            var damage = packet.Damage;
            var hp = packet.Hp;
            var maxhp = packet.MaxHp;

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithDamage(damage, hp == 0);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.NotifyTakeDamage(damage, (int)Math.Round(((double)hp / maxhp) * 100), isHeal: false);

            foreach (var notifier in _effectNotifiers)
                notifier.NotifyMapEffect(IO.Map.MapEffect.HPDrain);

            foreach (var other in packet.Others)
            {
                foreach (var notifier in _otherCharacterEventNotifiers)
                    notifier.OtherCharacterTakeDamage(other.PlayerId, other.HpPercentage, other.Damage, isHeal: false);
            }

            return true;
        }
    }
}