using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class PlayerSpikeDamageHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Admin;

        public PlayerSpikeDamageHandler(IPlayerInfoProvider playerInfoProvider,
                                        ICurrentMapStateRepository currentMapStateRepository,
                                        IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            short characterId = packet.ReadShort();
            byte playerPercentHealth = packet.ReadChar();
            bool isDead = packet.ReadChar() != 0;
            int damageTaken = packet.ReadThree();

            var characterToUpdate = _currentMapStateRepository.Characters.Single(x => x.ID == characterId);
            var updatedCharacter = characterToUpdate.WithDamage(damageTaken, isDead);

            _currentMapStateRepository.Characters.Remove(characterToUpdate);
            _currentMapStateRepository.Characters.Add(updatedCharacter);

            foreach (var notifier in _otherCharacterEventNotifiers)
            {
                notifier.OtherCharacterTakeDamage(characterId, playerPercentHealth, damageTaken);
            }

            return true;
        }
    }
}
