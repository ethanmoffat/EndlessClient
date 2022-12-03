using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Recover
{
    /// <summary>
    /// Sent when a player uses a heal item
    /// </summary>
    [AutoMappedType]
    public class RecoverAgreeHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var playerId = packet.ReadShort();
            var hpGain = packet.ReadInt();
            var percentHealth = packet.ReadChar();

            if (_characterRepository.MainCharacter.ID == playerId)
            {
                var c = _characterRepository.MainCharacter;
                var stats = c.Stats;
                var hp = Math.Min(stats[CharacterStat.HP] + hpGain, stats[CharacterStat.MaxHP]);
                _characterRepository.MainCharacter = c.WithStats(stats.WithNewStat(CharacterStat.HP, hp));

                foreach (var notifier in _mainCharacterEventNotifiers)
                    notifier.NotifyTakeDamage(hpGain, percentHealth, isHeal: true);
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(playerId))
            {
                foreach (var notifier in _otherCharacterEventNotifiers)
                    notifier.OtherCharacterTakeDamage(playerId, percentHealth, hpGain, isHeal: true);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerId);
            }

            return true;
        }
    }
}
