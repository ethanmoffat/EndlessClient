
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System;
using System.Collections.Generic;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class MapHpDrainHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.TargetOther;

        public MapHpDrainHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                 IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            short damage = packet.ReadShort();
            short hp = packet.ReadShort();
            short maxhp = packet.ReadShort();

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithDamage(damage, hp == 0);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.NotifyTakeDamage(damage, (int)Math.Round((double)hp / maxhp));

            while (packet.ReadPosition != packet.Length)
            {
                var otherCharacterId = packet.ReadShort();
                var otherCharacterPercentHealth = packet.ReadChar();
                var damageDealt = packet.ReadShort();

                foreach (var notifier in _otherCharacterEventNotifiers)
                    notifier.OtherCharacterTakeDamage(otherCharacterId, otherCharacterPercentHealth, damageDealt);
            }

            return true;
        }
    }
}
