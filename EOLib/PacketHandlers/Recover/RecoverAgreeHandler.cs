using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Recover
{
    /// <summary>
    /// Sent when a player uses a heal item
    /// </summary>
    [AutoMappedType]
    public class RecoverAgreeHandler : InGameOnlyPacketHandler<RecoverAgreeServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Recover;

        public override PacketAction Action => PacketAction.Agree;

        public RecoverAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICharacterRepository characterRepository,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                   IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
        }

        public override bool HandlePacket(RecoverAgreeServerPacket packet)
        {
            if (_characterRepository.MainCharacter.ID == packet.PlayerId)
            {
                var c = _characterRepository.MainCharacter;
                var stats = c.Stats;
                var hp = Math.Min(stats[CharacterStat.HP] + packet.HealHp, stats[CharacterStat.MaxHP]);
                _characterRepository.MainCharacter = c.WithStats(stats.WithNewStat(CharacterStat.HP, hp));

                foreach (var notifier in _mainCharacterEventNotifiers)
                    notifier.NotifyTakeDamage(packet.HealHp, packet.HpPercentage, isHeal: true);
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(packet.PlayerId))
            {
                foreach (var notifier in _otherCharacterEventNotifiers)
                    notifier.OtherCharacterTakeDamage(packet.PlayerId, packet.HpPercentage, packet.HealHp, isHeal: true);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
            }

            return true;
        }
    }
}
